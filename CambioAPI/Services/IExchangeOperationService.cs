using System.Threading.Tasks;
using CambioAPI.DTOs;
using CambioAPI.Models;

namespace CambioAPI.Services
{
    public interface IExchangeOperationService
    {
        Task<(bool success, string message, ExchangeOperationResponseDTO operation)> CreateOperationAsync(CreateExchangeOperationDTO model, int currentUserId);
        Task<(bool success, string message)> UpdateOperationStatusAsync(UpdateExchangeOperationStatusDTO model, int currentUserId);
        Task<ExchangeOperationResponseDTO> GetOperationByIdAsync(int operationId);
        Task<ExchangeOperationSearchResponseDTO> SearchOperationsAsync(ExchangeOperationSearchDTO searchParams);
        Task<bool> OperationExistsAsync(int operationId);
        Task<bool> CanUpdateOperationStatusAsync(int operationId, int currentUserId);
        Task<(bool success, string message, string ticket)> GenerateTicketAsync(int operationId, int currentUserId);
        Task<(bool success, string message, ExchangeOperationResponseDTO operation)> UpdateOperationAsync(int id, UpdateExchangeOperationDTO model, int currentUserId);
        Task<(bool success, string message)> DeleteOperationAsync(int id, int currentUserId);
        Task<ExchangeOperationSearchResponseDTO> GetPagedOperationsAsync(ExchangeOperationSearchDTO searchParams);
    }
} 