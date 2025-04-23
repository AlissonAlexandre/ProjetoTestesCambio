using CambioAPI.Data;
using CambioAPI.DTOs;
using CambioAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CambioAPI.Services
{
    public class CustomerLimitService : ICustomerLimitService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthService _authService;
        private readonly ICustomerService _customerService;

        public CustomerLimitService(
            ApplicationDbContext context,
            IAuthService authService,
            ICustomerService customerService)
        {
            _context = context;
            _authService = authService;
            _customerService = customerService;
        }

        public async Task<(bool success, string message, CustomerLimitResponseDTO limit)> CreateCustomerLimitAsync(CreateCustomerLimitDTO model, int currentUserId)
        {
            
            if (!await _authService.IsMasterUserAsync(currentUserId))
            {
                return (false, "Apenas usuários master podem cadastrar limites", null);
            }

            
            if (!await _customerService.CustomerExistsAsync(model.CustomerId))
            {
                return (false, "Cliente não encontrado", null);
            }

            
            if (await CustomerHasLimitAsync(model.CustomerId))
            {
                return (false, "Este cliente já possui um limite cadastrado", null);
            }

            var limit = new CustomerLimit
            {
                CustomerId = model.CustomerId,
                Limit = model.Limit,
                CreatedByUserId = currentUserId,
                CreatedAt = DateTime.UtcNow
            };

            _context.CustomerLimits.Add(limit);
            await _context.SaveChangesAsync();

            return (true, "Limite cadastrado com sucesso", await MapToResponseDTO(limit));
        }

        public async Task<(bool success, string message, CustomerLimitResponseDTO limit)> UpdateCustomerLimitAsync(UpdateCustomerLimitDTO model, int currentUserId)
        {
            
            if (!await _authService.IsMasterUserAsync(currentUserId))
            {
                return (false, "Apenas usuários master podem atualizar limites", null);
            }

            var limit = await _context.CustomerLimits
                .Include(l => l.Customer)
                .FirstOrDefaultAsync(l => l.Id == model.LimitId);

            if (limit == null)
            {
                return (false, "Limite não encontrado", null);
            }

            limit.Limit = model.NewLimit;
            limit.LastUpdatedByUserId = currentUserId;
            limit.LastUpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return (true, "Limite atualizado com sucesso", await MapToResponseDTO(limit));
        }

        public async Task<CustomerLimitResponseDTO> GetCustomerLimitByIdAsync(int limitId)
        {
            var limit = await _context.CustomerLimits
                .Include(l => l.Customer)
                .Include(l => l.CreatedByUser)
                .Include(l => l.LastUpdatedByUser)
                .FirstOrDefaultAsync(l => l.Id == limitId);

            return limit != null ? await MapToResponseDTO(limit) : null;
        }

        public async Task<List<CustomerLimitResponseDTO>> GetAllCustomerLimitsAsync(int customerId)
        {
            var limits = await _context.CustomerLimits
                .Include(l => l.Customer)
                .Include(l => l.CreatedByUser)
                .Include(l => l.LastUpdatedByUser)
                .Where(l => l.CustomerId == customerId)
                .ToListAsync();

            var dtos = new List<CustomerLimitResponseDTO>();
            foreach (var limit in limits)
            {
                dtos.Add(await MapToResponseDTO(limit));
            }

            return dtos;
        }

        public async Task<bool> CustomerLimitExistsAsync(int limitId)
        {
            return await _context.CustomerLimits.AnyAsync(l => l.Id == limitId);
        }

        public async Task<CustomerLimitResponseDTO> GetCustomerLimitAsync(int customerId)
        {
            var limit = await _context.CustomerLimits
                .Include(l => l.Customer)
                .Include(l => l.CreatedByUser)
                .Include(l => l.LastUpdatedByUser)
                .FirstOrDefaultAsync(l => l.CustomerId == customerId);

            return limit != null ? await MapToResponseDTO(limit) : null;
        }

        public async Task<bool> HasSufficientLimitAsync(int customerId, decimal amount)
        {
            var limit = await _context.CustomerLimits
                .FirstOrDefaultAsync(l => l.CustomerId == customerId);

            if (limit == null)
                return false;

            return amount <= limit.Limit;
        }

        public async Task<bool> CustomerHasLimitAsync(int customerId)
        {
            return await _context.CustomerLimits.AnyAsync(l => l.CustomerId == customerId);
        }

        public async Task<(bool success, string message)> UpdateLimitAsync(int customerId, decimal amount, bool isAddition)
        {
            try
            {
                var limit = await _context.CustomerLimits
                    .FirstOrDefaultAsync(cl => cl.CustomerId == customerId);

                if (limit == null)
                {
                    return (false, "Limite do cliente não encontrado");
                }

                if (isAddition)
                {
                    limit.Limit += amount;
                }
                else
                {
                    if (limit.Limit < amount)
                    {
                        return (false, "Limite insuficiente para a operação");
                    }
                    limit.Limit -= amount;
                }

                await _context.SaveChangesAsync();
                return (true, "Limite atualizado com sucesso");
            }
            catch (Exception ex)
            {
                return (false, $"Erro ao atualizar o limite: {ex.Message}");
            }
        }

        private async Task<CustomerLimitResponseDTO> MapToResponseDTO(CustomerLimit limit)
        {
            return new CustomerLimitResponseDTO
            {
                CustomerId = limit.CustomerId,
                CustomerName = limit.Customer?.Name,
                CustomerDocument = limit.Customer?.Document,
                Limit = limit.Limit,
                CreatedAt = limit.CreatedAt,
                LastUpdatedAt = limit.LastUpdatedAt,
                CreatedByUserName = limit.CreatedByUser?.Name,
                LastUpdatedByUserName = limit.LastUpdatedByUser?.Name
            };
        }
    }
} 