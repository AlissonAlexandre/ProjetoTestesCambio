using System.Threading.Tasks;
using CambioAPI.Models;
using CambioAPI.DTOs;

namespace CambioAPI.Services
{
    public interface IAuthService
    {
        Task<(bool success, string message, User user, string token)> RegisterAsync(UserRegistrationDTO model);
        Task<(bool success, string message, User user, string token)> LoginAsync(UserLoginDTO model);
        Task<bool> IsEmailUniqueAsync(string email);
        Task<bool> IsMasterUserAsync(int userId);
        Task<(bool success, string message)> UpdateUserRoleAsync(UpdateUserRoleDTO model, int currentUserId);
        Task<bool> HasAnyMasterUserAsync();
        Task CreateFirstMasterUserAsync(UserRegistrationDTO model);
        Task<(bool success, string message, string token)> RefreshTokenAsync(string refreshToken);

        Task<List<UserResponseDTO>> GetAllUsersAsync();
    }
} 