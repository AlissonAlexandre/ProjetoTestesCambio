using System.Threading.Tasks;
using CambioAPI.DTOs;
using CambioAPI.Models;

namespace CambioAPI.Services
{
    public interface IUserService
    {
        Task<(bool success, string message, User user)> CreateUserAsync(CreateUserDTO userDTO);
        Task<(bool success, string message, User user)> GetUserByIdAsync(int id);
        Task<(bool success, string message, User user)> GetUserByEmailAsync(string email);
        Task<bool> UserExistsAsync(int id);
        Task<bool> EmailExistsAsync(string email);
        Task<(bool success, string message)> UpdatePasswordAsync(int userId, UpdatePasswordDTO passwordDTO);
        Task<UserProfileDTO> GetUserProfileAsync(int userId);
        Task<(bool success, string message, UserProfileDTO profile)> UpdateProfileAsync(int userId, UpdateProfileDTO profileDTO);
    }
} 