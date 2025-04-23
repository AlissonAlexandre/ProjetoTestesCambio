using System.Threading.Tasks;
using CambioAPI.DTOs;
using CambioAPI.Models;

namespace CambioAPI.Services
{
    public interface ICustomerDocumentService
    {
        Task<(bool success, string message, CustomerDocument document)> CreateDocumentAsync(CreateCustomerDocumentDTO dto);
        Task<PagedResponseDTO<CustomerDocumentDTO>> GetCustomerDocumentsAsync(int? customerId, PaginationDTO pagination);
    }
} 