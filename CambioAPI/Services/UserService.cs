using System;
using System.Threading.Tasks;
using CambioAPI.Data;
using CambioAPI.DTOs;
using CambioAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;

namespace CambioAPI.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserService> _logger;

        public UserService(ApplicationDbContext context, ILogger<UserService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<(bool success, string message, User user)> CreateUserAsync(CreateUserDTO userDTO)
        {
            if (await EmailExistsAsync(userDTO.Email))
            {
                return (false, "Já existe um usuário cadastrado com este e-mail", null);
            }

            var user = new User
            {
                Name = userDTO.Name,
                Email = userDTO.Email,
                Password = HashPassword(userDTO.Password),
                IsMaster = userDTO.IsMaster,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return (true, "Usuário cadastrado com sucesso", user);
        }

        public async Task<(bool success, string message, User user)> GetUserByIdAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return (false, $"Usuário {id} não encontrado", null);
            }

            return (true, null, user);
        }

        public async Task<(bool success, string message, User user)> GetUserByEmailAsync(string email)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return (false, $"Usuário com e-mail {email} não encontrado", null);
            }

            return (true, null, user);
        }

        public async Task<bool> UserExistsAsync(int id)
        {
            return await _context.Users.AnyAsync(u => u.Id == id);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<(bool success, string message)> UpdatePasswordAsync(int userId, UpdatePasswordDTO passwordDTO)
        {
            try
            {
                _logger.LogInformation($"Buscando usuário com ID: {userId}");
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning($"Usuário não encontrado: {userId}");
                    return (false, "Usuário não encontrado");
                }

                _logger.LogInformation("Verificando senha atual");
                
                if (user.Password != HashPassword(passwordDTO.CurrentPassword))
                {
                    _logger.LogWarning("Senha atual incorreta");
                    return (false, "Senha atual incorreta");
                }

                
                if (string.IsNullOrEmpty(passwordDTO.NewPassword) || passwordDTO.NewPassword.Length < 6)
                {
                    _logger.LogWarning("Nova senha inválida");
                    return (false, "A nova senha deve ter no mínimo 6 caracteres");
                }

                _logger.LogInformation("Atualizando senha");
                
                user.Password = HashPassword(passwordDTO.NewPassword);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Senha atualizada com sucesso");
                return (true, "Senha atualizada com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar senha");
                return (false, $"Erro ao atualizar senha: {ex.Message}");
            }
        }

        public async Task<UserProfileDTO> GetUserProfileAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return null;
            }

            return new UserProfileDTO
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                IsMaster = user.IsMaster,
                CreatedAt = user.CreatedAt
            };
        }

        public async Task<(bool success, string message, UserProfileDTO profile)> UpdateProfileAsync(int userId, UpdateProfileDTO profileDTO)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return (false, "Usuário não encontrado", null);
            }

            
            if (!string.IsNullOrEmpty(profileDTO.Email) && profileDTO.Email != user.Email)
            {
                if (await EmailExistsAsync(profileDTO.Email))
                {
                    return (false, "Já existe um usuário cadastrado com este e-mail", null);
                }
                user.Email = profileDTO.Email;
            }

            
            if (!string.IsNullOrEmpty(profileDTO.Name))
            {
                user.Name = profileDTO.Name;
            }

            await _context.SaveChangesAsync();

            return (true, "Perfil atualizado com sucesso", new UserProfileDTO
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                IsMaster = user.IsMaster,
                CreatedAt = user.CreatedAt
            });
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
} 