using System;
using System.Threading.Tasks;
using CambioAPI.Data;
using CambioAPI.DTOs;
using CambioAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CambioAPI.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthService _authService;

        public CustomerService(ApplicationDbContext context, IAuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        public async Task<(bool success, string message, Customer customer)> CreateCustomerAsync(CustomerCreateDTO customerDTO, int createdByUserId)
        {
            if (!await _authService.IsMasterUserAsync(createdByUserId))
            {
                return (false, "Apenas usuários master podem cadastrar clientes", null);
            }

            if (await DocumentExistsAsync(customerDTO.Document))
            {
                return (false, "Já existe um cliente cadastrado com este documento", null);
            }

            var customer = new Customer
            {
                Name = customerDTO.Name,
                Document = customerDTO.Document,
                Phone = customerDTO.Phone,
                Email = customerDTO.Email,
                CreatedByUserId = createdByUserId,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Customers.AddAsync(customer);
            await _context.SaveChangesAsync();

            return (true, "Cliente cadastrado com sucesso", customer);
        }

        public async Task<(bool success, string message, Customer customer)> UpdateCustomerAsync(int id, CustomerCreateDTO customerDTO)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return (false, "Cliente não encontrado", null);
            }

            var existingCustomer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Document == customerDTO.Document && c.Id != id);

            customer.Name = customerDTO.Name;
            customer.Document = customerDTO.Document;
            customer.Phone = customerDTO.Phone;
            customer.Email = customerDTO.Email;

            await _context.SaveChangesAsync();

            return (true, "Cliente atualizado com sucesso", customer);
        }

        public async Task<(bool success, string message, Customer customer)> GetCustomerByIdAsync(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return (false, $"Cliente {id} não encontrado", null);
            }

            return (true, null, customer);
        }

        public async Task<(bool success, string message, Customer customer)> GetCustomerByDocumentAsync(string document)
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Document == document);
            if (customer == null)
            {
                return (false, $"Cliente com documento {document} não encontrado", null);
            }

            return (true, null, customer);
        }

        public async Task<(bool success, string message, List<Customer> customers)> SearchCustomersAsync(string searchTerm = null)
        {
            var query = _context.Customers.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(c =>
                    c.Name.ToLower().Contains(searchTerm) ||
                    c.Document.ToLower().Contains(searchTerm) ||
                    c.Email.ToLower().Contains(searchTerm));
            }

            var customers = await query.ToListAsync();
            return (true, null, customers);
        }

        public async Task<bool> CustomerExistsAsync(int id)
        {
            return await _context.Customers.AnyAsync(c => c.Id == id);
        }

        public async Task<bool> DocumentExistsAsync(string document)
        {
            return await _context.Customers.AnyAsync(c => c.Document == document);
        }

        public async Task<PaginatedResponse<CustomerResponseDTO>> GetPaginatedCustomersAsync(int pageNumber = 1, int pageSize = 10)
        {
            var query = _context.Customers
                .Include(c => c.CustomerLimit)
                .AsNoTracking();

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var customers = await query
                .OrderBy(c => c.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CustomerResponseDTO
                {
                    Id = c.Id,
                    Name = c.Name,
                    Document = c.Document,
                    DocumentType = c.DocumentType,
                    Phone = c.Phone,
                    Limit = c.CustomerLimit != null ? c.CustomerLimit.Limit : null,
                    CreatedAt = c.CreatedAt,
                    Email = c.Email,
                })
                .ToListAsync();

            return new PaginatedResponse<CustomerResponseDTO>
            {
                Items = customers,
                TotalItems = totalItems,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages,
                HasPreviousPage = pageNumber > 1,
                HasNextPage = pageNumber < totalPages
            };
        }

        public async Task<(bool success, string message)> DeleteCustomerAsync(int id)
        {
            try
            {
                
                var hasOperations = await _context.ExchangeOperations
                    .AnyAsync(eo => eo.CustomerId == id);

                if (hasOperations)
                {
                    return (false, "Não é possível excluir o cliente pois ele possui operações de câmbio registradas");
                }

                
                var customerLimit = await _context.CustomerLimits
                    .FirstOrDefaultAsync(cl => cl.CustomerId == id);

                
                if (customerLimit != null)
                {
                    _context.CustomerLimits.Remove(customerLimit);
                    await _context.SaveChangesAsync();
                }

                
                var customer = await _context.Customers.FindAsync(id);
                if (customer == null)
                {
                    return (false, "Cliente não encontrado");
                }

                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();

                return (true, "Cliente excluído com sucesso");
            }
            catch (DbUpdateException ex)
            {
                return (false, "Erro ao excluir o cliente. Verifique se existem registros relacionados.");
            }
            catch (Exception ex)
            {
                return (false, $"Erro inesperado ao excluir o cliente: {ex.Message}");
            }
        }
    }
} 