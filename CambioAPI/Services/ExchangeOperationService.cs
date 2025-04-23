using CambioAPI.Data;
using CambioAPI.DTOs;
using CambioAPI.Models;
using CambioAPI.Enums;
using Microsoft.EntityFrameworkCore;

namespace CambioAPI.Services
{
    public class ExchangeOperationService : IExchangeOperationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthService _authService;
        private readonly ICurrencyService _currencyService;
        private readonly ICustomerService _customerService;
        private readonly ICustomerLimitService _customerLimitService;

        public ExchangeOperationService(
            ApplicationDbContext context,
            IAuthService authService,
            ICurrencyService currencyService,
            ICustomerService customerService,
            ICustomerLimitService customerLimitService)
        {
            _context = context;
            _authService = authService;
            _currencyService = currencyService;
            _customerService = customerService;
            _customerLimitService = customerLimitService;
        }

        public async Task<(bool success, string message, ExchangeOperationResponseDTO operation)> CreateOperationAsync(CreateExchangeOperationDTO model, int currentUserId)
        {
            try
            {
                if (!await _customerService.CustomerExistsAsync(model.CustomerId))
                {
                    return (false, "Cliente não encontrado", null);
                }

                var fromCurrencyResult = await _currencyService.GetCurrencyByCodeAsync(model.FromCurrencyCode);
                if (!fromCurrencyResult.success)
                {
                    return (false, fromCurrencyResult.message, null);
                }

                var toCurrencyResult = await _currencyService.GetCurrencyByCodeAsync(model.ToCurrencyCode);
                if (!toCurrencyResult.success)
                {
                    return (false, toCurrencyResult.message, null);
                }

                var fromCurrency = fromCurrencyResult.currency;
                var toCurrency = toCurrencyResult.currency;

                var quoteResult = await _currencyService.GetQuoteAsync(fromCurrency.Id, toCurrency.Id);
                if (!quoteResult.success)
                {
                    return (false, quoteResult.message, null);
                }

                var finalAmount = model.Amount * quoteResult.rate;

                
                if (!await _customerLimitService.HasSufficientLimitAsync(model.CustomerId, finalAmount))
                {
                    return (false, "Cliente não possui limite suficiente para esta operação", null);
                }

                
                var updateLimitResult = await _customerLimitService.UpdateLimitAsync(model.CustomerId, finalAmount, false);
                if (!updateLimitResult.success)
                {
                    return (false, updateLimitResult.message, null);
                }

                var operation = new ExchangeOperation
                {
                    CustomerId = model.CustomerId,
                    FromCurrencyId = fromCurrency.Id,
                    ToCurrencyId = toCurrency.Id,
                    Amount = model.Amount,
                    ExchangeRate = quoteResult.rate,
                    FinalAmount = finalAmount,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = currentUserId,
                    Status = OperationStatus.Completed 
                };

                _context.ExchangeOperations.Add(operation);
                await _context.SaveChangesAsync();

                return (true, "Operação de câmbio criada com sucesso", await MapToResponseDTO(operation));
            }
            catch (Exception ex)
            {
                return (false, $"Erro ao criar a operação: {ex.Message}", null);
            }
        }

        public async Task<(bool success, string message)> UpdateOperationStatusAsync(UpdateExchangeOperationStatusDTO model, int currentUserId)
        {
            
            if (!await _authService.IsMasterUserAsync(currentUserId))
            {
                return (false, "Apenas usuários master podem atualizar o status das operações");
            }

            var operation = await _context.ExchangeOperations.FindAsync(model.OperationId);
            if (operation == null)
            {
                return (false, "Operação não encontrada");
            }

            
            if (!CanUpdateStatus(operation.Status, model.NewStatus))
            {
                return (false, "Não é possível atualizar o status da operação para o status solicitado");
            }

            operation.Status = model.NewStatus;
            await _context.SaveChangesAsync();

            return (true, "Status da operação atualizado com sucesso");
        }

        public async Task<ExchangeOperationResponseDTO> GetOperationByIdAsync(int operationId)
        {
            var operation = await _context.ExchangeOperations
                .Include(o => o.Customer)
                .Include(o => o.FromCurrency)
                .Include(o => o.ToCurrency)
                .Include(o => o.CreatedByUser)
                .FirstOrDefaultAsync(o => o.Id == operationId);

            return operation != null ? await MapToResponseDTO(operation) : null;
        }

        public async Task<ExchangeOperationSearchResponseDTO> SearchOperationsAsync(ExchangeOperationSearchDTO searchParams)
        {
            var query = _context.ExchangeOperations
                .Include(eo => eo.Customer)
                .Include(eo => eo.FromCurrency)
                .Include(eo => eo.ToCurrency)
                .Include(eo => eo.CreatedByUser)
                .AsQueryable();

            
            if (searchParams.StartDate.HasValue)
            {
                query = query.Where(eo => eo.CreatedAt >= searchParams.StartDate.Value);
            }

            if (searchParams.EndDate.HasValue)
            {
                query = query.Where(eo => eo.CreatedAt <= searchParams.EndDate.Value);
            }

            if (searchParams.CustomerId.HasValue)
            {
                query = query.Where(eo => eo.CustomerId == searchParams.CustomerId.Value);
            }

            if (searchParams.Status.HasValue)
            {
                query = query.Where(eo => eo.Status == searchParams.Status.Value);
            }

            
            var totalCount = await query.CountAsync();
            var totalAmount = await query.SumAsync(eo => eo.FinalAmount);
            var totalPages = (int)Math.Ceiling(totalCount / (double)searchParams.PageSize);

            
            var operations = await query
                .OrderByDescending(eo => eo.CreatedAt)
                .Skip((searchParams.PageNumber - 1) * searchParams.PageSize)
                .Take(searchParams.PageSize)
                .Select(eo => new ExchangeOperationDTO
                {
                    Id = eo.Id,
                    CustomerId = eo.CustomerId,
                    CustomerName = eo.Customer.Name,
                    FromCurrencyId = eo.FromCurrencyId,
                    FromCurrencyCode = eo.FromCurrency.Code,
                    ToCurrencyId = eo.ToCurrencyId,
                    ToCurrencyCode = eo.ToCurrency.Code,
                    Amount = eo.Amount,
                    ExchangeRate = eo.ExchangeRate,
                    FinalAmount = eo.FinalAmount,
                    Status = eo.Status,
                    CreatedAt = eo.CreatedAt,
                    CreatedByUserId = eo.CreatedByUserId,
                    CreatedByUserName = eo.CreatedByUser.Name
                })
                .ToListAsync();

            return new ExchangeOperationSearchResponseDTO
            {
                Operations = operations,
                PageNumber = searchParams.PageNumber,
                PageSize = searchParams.PageSize,
                TotalPages = totalPages,
                TotalCount = totalCount,
                TotalAmount = totalAmount
            };
        }

        public async Task<bool> OperationExistsAsync(int operationId)
        {
            return await _context.ExchangeOperations.AnyAsync(o => o.Id == operationId);
        }

        public async Task<bool> CanUpdateOperationStatusAsync(int operationId, int currentUserId)
        {
            if (!await _authService.IsMasterUserAsync(currentUserId))
                return false;

            var operation = await _context.ExchangeOperations.FindAsync(operationId);
            if (operation == null)
                return false;

            return operation.Status != OperationStatus.Deleted &&
                   operation.Status != OperationStatus.Deleted;
        }

        public async Task<(bool success, string message, string ticket)> GenerateTicketAsync(int operationId, int currentUserId)
        {
            var operation = await _context.ExchangeOperations
                .Include(o => o.Customer)
                .Include(o => o.FromCurrency)
                .Include(o => o.ToCurrency)
                .FirstOrDefaultAsync(o => o.Id == operationId);

            if (operation == null)
            {
                return (false, "Operação não encontrada", null);
            }

            
            if (operation.CreatedByUserId != currentUserId && !await _authService.IsMasterUserAsync(currentUserId))
            {
                return (false, "Você não tem permissão para gerar o ticket desta operação", null);
            }

            
            var ticketData = $"{operation.Id}-{operation.CustomerId}-{operation.FromCurrencyId}-{operation.ToCurrencyId}-{operation.Amount}-{operation.CreatedAt.Ticks}";
            var ticket = Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(ticketData)));

            return (true, "Ticket gerado com sucesso", ticket);
        }

        public async Task<(bool success, string message, ExchangeOperationResponseDTO operation)> UpdateOperationAsync(int id, UpdateExchangeOperationDTO model, int currentUserId)
        {
            try
            {
                var operation = await _context.ExchangeOperations
                    .Include(o => o.Customer)
                    .Include(o => o.FromCurrency)
                    .Include(o => o.ToCurrency)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (operation == null)
                {
                    return (false, "Operação não encontrada", null);
                }

                
                if (operation.CreatedByUserId != currentUserId && !await _authService.IsMasterUserAsync(currentUserId))
                {
                    return (false, "Você não tem permissão para editar esta operação", null);
                }

                
                if (operation.Status == OperationStatus.Deleted)
                {
                    return (false, "Não é possível editar uma operação excluída", null);
                }

                
                if (!await _customerService.CustomerExistsAsync(model.CustomerId))
                {
                    return (false, "Cliente não encontrado", null);
                }

                
                var fromCurrencyResult = await _currencyService.GetCurrencyByCodeAsync(model.FromCurrencyCode);
                if (!fromCurrencyResult.success)
                {
                    return (false, fromCurrencyResult.message, null);
                }

                var toCurrencyResult = await _currencyService.GetCurrencyByCodeAsync(model.ToCurrencyCode);
                if (!toCurrencyResult.success)
                {
                    return (false, toCurrencyResult.message, null);
                }

                var fromCurrency = fromCurrencyResult.currency;
                var toCurrency = toCurrencyResult.currency;

                
                var quoteResult = await _currencyService.GetQuoteAsync(fromCurrency.Id, toCurrency.Id);
                if (!quoteResult.success)
                {
                    return (false, quoteResult.message, null);
                }

                var newFinalAmount = model.Amount * quoteResult.rate;

                
                var restoreLimitResult = await _customerLimitService.UpdateLimitAsync(operation.CustomerId, operation.FinalAmount, true);
                if (!restoreLimitResult.success)
                {
                    return (false, restoreLimitResult.message, null);
                }

                
                if (!await _customerLimitService.HasSufficientLimitAsync(model.CustomerId, newFinalAmount))
                {
                    
                    await _customerLimitService.UpdateLimitAsync(operation.CustomerId, operation.FinalAmount, false);
                    return (false, "Cliente não possui limite suficiente para esta operação", null);
                }

                
                var updateLimitResult = await _customerLimitService.UpdateLimitAsync(model.CustomerId, newFinalAmount, false);
                if (!updateLimitResult.success)
                {
                    
                    await _customerLimitService.UpdateLimitAsync(operation.CustomerId, operation.FinalAmount, false);
                    return (false, updateLimitResult.message, null);
                }

                
                operation.CustomerId = model.CustomerId;
                operation.FromCurrencyId = fromCurrency.Id;
                operation.ToCurrencyId = toCurrency.Id;
                operation.Amount = model.Amount;
                operation.ExchangeRate = quoteResult.rate;
                operation.FinalAmount = newFinalAmount;
                operation.Status = OperationStatus.Modified; 

                await _context.SaveChangesAsync();

                return (true, "Operação atualizada com sucesso", await MapToResponseDTO(operation));
            }
            catch (Exception ex)
            {
                return (false, $"Erro ao atualizar a operação: {ex.Message}", null);
            }
        }

        public async Task<(bool success, string message)> DeleteOperationAsync(int id, int currentUserId)
        {
            try
            {
                var operation = await _context.ExchangeOperations
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (operation == null)
                {
                    return (false, "Operação não encontrada");
                }

                
                if (operation.CreatedByUserId != currentUserId && !await _authService.IsMasterUserAsync(currentUserId))
                {
                    return (false, "Você não tem permissão para excluir esta operação");
                }

                if (operation.Status == OperationStatus.Deleted)
                {
                    return (false, "Esta operação já está excluída");
                }

                
                var restoreLimitResult = await _customerLimitService.UpdateLimitAsync(operation.CustomerId, operation.FinalAmount, true);
                if (!restoreLimitResult.success)
                {
                    return (false, restoreLimitResult.message);
                }

                
                operation.Status = OperationStatus.Deleted;
                await _context.SaveChangesAsync();

                return (true, "Operação excluída com sucesso");
            }
            catch (Exception ex)
            {
                return (false, $"Erro ao excluir a operação: {ex.Message}");
            }
        }

        private bool CanUpdateStatus(OperationStatus currentStatus, OperationStatus newStatus)
        {
            if (currentStatus == OperationStatus.Deleted)
                return false;

            return true;
        }

        private async Task<ExchangeOperationResponseDTO> MapToResponseDTO(ExchangeOperation operation)
        {
            return new ExchangeOperationResponseDTO
            {
                Id = operation.Id,
                CustomerId = operation.CustomerId,
                CustomerName = operation.Customer?.Name,
                CustomerDocument = operation.Customer?.Document,
                FromCurrency = operation.FromCurrency?.Code,
                ToCurrency = operation.ToCurrency?.Code,
                Amount = operation.Amount,
                ExchangeRate = operation.ExchangeRate,
                FinalAmount = operation.FinalAmount,
                CreatedAt = operation.CreatedAt,
                CreatedByUserName = operation.CreatedByUser?.Name,
                Status = operation.Status
            };
        }

        public async Task<ExchangeOperationSearchResponseDTO> GetPagedOperationsAsync(ExchangeOperationSearchDTO searchParams)
        {
            var query = _context.ExchangeOperations
                .Include(eo => eo.Customer)
                .Include(eo => eo.FromCurrency)
                .Include(eo => eo.ToCurrency)
                .Include(eo => eo.CreatedByUser)
                .AsQueryable();

            if (searchParams.StartDate.HasValue)
            {
                query = query.Where(eo => eo.CreatedAt >= searchParams.StartDate.Value);
            }

            if (searchParams.EndDate.HasValue)
            {
                query = query.Where(eo => eo.CreatedAt <= searchParams.EndDate.Value);
            }

            if (searchParams.CustomerId.HasValue)
            {
                query = query.Where(eo => eo.CustomerId == searchParams.CustomerId.Value);
            }

            if (searchParams.Status.HasValue)
            {
                query = query.Where(eo => eo.Status == searchParams.Status.Value);
            }

            query = searchParams.SortBy?.ToLower() switch
            {
                "id" => searchParams.Ascending ? query.OrderBy(eo => eo.Id) : query.OrderByDescending(eo => eo.Id),
                "customername" => searchParams.Ascending ? query.OrderBy(eo => eo.Customer.Name) : query.OrderByDescending(eo => eo.Customer.Name),
                "amount" => searchParams.Ascending ? query.OrderBy(eo => eo.Amount) : query.OrderByDescending(eo => eo.Amount),
                "finalamount" => searchParams.Ascending ? query.OrderBy(eo => eo.FinalAmount) : query.OrderByDescending(eo => eo.FinalAmount),
                "status" => searchParams.Ascending ? query.OrderBy(eo => eo.Status) : query.OrderByDescending(eo => eo.Status),
                _ => searchParams.Ascending ? query.OrderBy(eo => eo.CreatedAt) : query.OrderByDescending(eo => eo.CreatedAt)
            };

            var totalCount = await query.CountAsync();
            var totalAmount = await query.SumAsync(eo => eo.FinalAmount);
            var totalPages = (int)Math.Ceiling(totalCount / (double)searchParams.PageSize);

            var operations = await query
                .Skip((searchParams.PageNumber - 1) * searchParams.PageSize)
                .Take(searchParams.PageSize)
                .Select(eo => new ExchangeOperationDTO
                {
                    Id = eo.Id,
                    CustomerId = eo.CustomerId,
                    CustomerName = eo.Customer.Name,
                    FromCurrencyId = eo.FromCurrencyId,
                    FromCurrencyCode = eo.FromCurrency.Code,
                    ToCurrencyId = eo.ToCurrencyId,
                    ToCurrencyCode = eo.ToCurrency.Code,
                    Amount = eo.Amount,
                    ExchangeRate = eo.ExchangeRate,
                    FinalAmount = eo.FinalAmount,
                    Status = eo.Status,
                    CreatedAt = eo.CreatedAt,
                    CreatedByUserId = eo.CreatedByUserId,
                    CreatedByUserName = eo.CreatedByUser.Name
                })
                .ToListAsync();

            return new ExchangeOperationSearchResponseDTO
            {
                Operations = operations,
                PageNumber = searchParams.PageNumber,
                PageSize = searchParams.PageSize,
                TotalPages = totalPages,
                TotalCount = totalCount,
                TotalAmount = totalAmount,
                HasPreviousPage = searchParams.PageNumber > 1,
                HasNextPage = searchParams.PageNumber < totalPages
            };
        }
    }
} 