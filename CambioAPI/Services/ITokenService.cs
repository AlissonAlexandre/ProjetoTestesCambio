using System.Security.Claims;
using CambioAPI.Models;

namespace CambioAPI.Services
{
    public interface ITokenService
    {
        string GenerateToken(User user);
        ClaimsPrincipal ValidateToken(string token);
    }
} 