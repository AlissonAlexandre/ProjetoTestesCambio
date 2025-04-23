using CambioAPI.DTOs;
using CambioAPI.Models;

namespace CambioAPI.Services
{
    public interface ICustomerLimitService
    {
        Task<(bool success, string message, CustomerLimitResponseDTO limit)> CreateCustomerLimitAsync(CreateCustomerLimitDTO model, int currentUserId);
        Task<(bool success, string message, CustomerLimitResponseDTO limit)> UpdateCustomerLimitAsync(UpdateCustomerLimitDTO model, int currentUserId);
        Task<CustomerLimitResponseDTO> GetCustomerLimitByIdAsync(int limitId);
        Task<List<CustomerLimitResponseDTO>> GetAllCustomerLimitsAsync(int customerId);
        Task<bool> CustomerLimitExistsAsync(int limitId);
        Task<bool> HasSufficientLimitAsync(int customerId, decimal amount);
        Task<bool> CustomerHasLimitAsync(int customerId);
        Task<(bool success, string message)> UpdateLimitAsync(int customerId, decimal amount, bool isAddition);
    }
} 