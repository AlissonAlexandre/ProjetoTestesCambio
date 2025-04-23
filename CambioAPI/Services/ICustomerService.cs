using System.Threading.Tasks;
using CambioAPI.DTOs;
using CambioAPI.Models;

namespace CambioAPI.Services
{
    public interface ICustomerService
    {
        Task<(bool success, string message, Customer customer)> CreateCustomerAsync(CustomerCreateDTO customerDTO, int createdByUserId);
        Task<(bool success, string message, Customer customer)> UpdateCustomerAsync(int id, CustomerCreateDTO customerDTO);
        Task<(bool success, string message, Customer customer)> GetCustomerByIdAsync(int id);
        Task<(bool success, string message, Customer customer)> GetCustomerByDocumentAsync(string document);
        Task<(bool success, string message, List<Customer> customers)> SearchCustomersAsync(string searchTerm = null);
        Task<bool> CustomerExistsAsync(int id);
        Task<bool> DocumentExistsAsync(string document);
        Task<PaginatedResponse<CustomerResponseDTO>> GetPaginatedCustomersAsync(int pageNumber = 1, int pageSize = 10);
        Task<(bool success, string message)> DeleteCustomerAsync(int id);
    }
} 