using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CambioAPI.Data;
using CambioAPI.DTOs;
using CambioAPI.Models;

namespace CambioAPI.Services
{
    public class CustomerDocumentService : ICustomerDocumentService
    {
        private readonly ApplicationDbContext _context;

        public CustomerDocumentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(bool success, string message, CustomerDocument document)> CreateDocumentAsync(CreateCustomerDocumentDTO dto)
        {
            var customer = await _context.Customers.FindAsync(dto.CustomerId);
            if (customer == null)
            {
                return (false, "Cliente não encontrado", null);
            }

            var document = new CustomerDocument
            {
                CustomerId = dto.CustomerId,
                Document = dto.Document,
                CreatedAt = DateTime.UtcNow
            };

            await _context.CustomerDocuments.AddAsync(document);
            await _context.SaveChangesAsync();

            return (true, "Documento criado com sucesso", document);
        }

        public async Task<PagedResponseDTO<CustomerDocumentDTO>> GetCustomerDocumentsAsync(int? customerId, PaginationDTO pagination)
        {
            var query = _context.CustomerDocuments
                .Include(d => d.Customer)
                .AsQueryable();

            if (customerId.HasValue)
            {
                query = query.Where(d => d.CustomerId == customerId.Value);
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pagination.PageSize);

            var documents = await query
                .OrderByDescending(d => d.CreatedAt)
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .Select(d => new CustomerDocumentDTO
                {
                    Id = d.Id,
                    CustomerId = d.CustomerId,
                    CustomerName = d.Customer.Name,
                    Document = d.Document,
                    CreatedAt = d.CreatedAt
                })
                .ToListAsync();

            return new PagedResponseDTO<CustomerDocumentDTO>
            {
                Items = documents,
                PageNumber = pagination.PageNumber,
                PageSize = pagination.PageSize,
                TotalPages = totalPages,
                TotalCount = totalCount
            };
        }
    }
} 