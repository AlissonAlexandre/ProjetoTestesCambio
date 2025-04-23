using System;
using System.Threading.Tasks;
using CambioAPI.Data;
using CambioAPI.Models;
using CambioAPI.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CambioAPI.Configuration;

namespace CambioAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtConfig _jwtConfig;

        public AuthService(ApplicationDbContext context, JwtConfig jwtConfig)
        {
            _context = context;
            _jwtConfig = jwtConfig;
        }

        public async Task<(bool success, string message, User user, string token)> RegisterAsync(UserRegistrationDTO model)
        {
            if (!await IsEmailUniqueAsync(model.Email))
            {
                return (false, "Email já cadastrado", null, null);
            }

            var user = new User
            {
                Name = model.Name,
                Email = model.Email,
                Password = HashPassword(model.Password),
                IsMaster = false,
                CreatedAt = DateTime.UtcNow
            };

            if (!user.IsValidPassword(model.Password))
            {
                return (false, "A senha deve conter letras e números e ter entre 6 e 20 caracteres", null, null);
            }

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var token = GenerateJwtToken(user);
            return (true, "Cadastro realizado com sucesso", user, token);
        }

        public async Task<List<UserResponseDTO>> GetAllUsersAsync()
        {
            var users = await _context.Users.ToListAsync();
            return users.Select(user => new UserResponseDTO
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                IsMaster = user.IsMaster
            }).ToList();
        }

        public async Task<(bool success, string message, User user, string token)> LoginAsync(UserLoginDTO model)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user == null || !VerifyPassword(model.Password, user.Password))
            {
                return (false, "Email ou senha inválidos", null, null);
            }

            var token = GenerateJwtToken(user);
            return (true, "Login realizado com sucesso", user, token);
        }

        public async Task<bool> IsEmailUniqueAsync(string email)
        {
            return !await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<bool> IsMasterUserAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            return user?.IsMaster ?? false;
        }

        public async Task<(bool success, string message)> UpdateUserRoleAsync(UpdateUserRoleDTO model, int currentUserId)
        {
            
            if (!await IsMasterUserAsync(currentUserId))
            {
                return (false, "Apenas usuários master podem alterar papéis de usuários");
            }

            
            if (model.UserId == currentUserId)
            {
                return (false, "Não é permitido alterar seu próprio papel no sistema");
            }

            var user = await _context.Users.FindAsync(model.UserId);
            if (user == null)
            {
                return (false, "Usuário não encontrado");
            }

            
            if (user.IsMaster && !model.IsMaster)
            {
                var masterCount = await _context.Users.CountAsync(u => u.IsMaster);
                if (masterCount <= 1)
                {
                    return (false, "Não é possível remover o último usuário master do sistema");
                }
            }

            user.IsMaster = model.IsMaster;
            await _context.SaveChangesAsync();

            return (true, $"Papel do usuário atualizado com sucesso para {(model.IsMaster ? "Master" : "Comum")}");
        }

        public async Task<bool> HasAnyMasterUserAsync()
        {
            return await _context.Users.AnyAsync(u => u.IsMaster);
        }

        public async Task CreateFirstMasterUserAsync(UserRegistrationDTO model)
        {
            if (await HasAnyMasterUserAsync())
            {
                throw new InvalidOperationException("Já existe um usuário master cadastrado");
            }

            var user = new User
            {
                Name = model.Name,
                Email = model.Email,
                Password = HashPassword(model.Password),
                IsMaster = true,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task<(bool success, string message, string token)> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_jwtConfig.Secret);
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false, 
                    ValidateAudience = false, 
                    ValidateLifetime = false, 
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(refreshToken, tokenValidationParameters, out var validatedToken);
                var jwtToken = validatedToken as JwtSecurityToken;

                if (jwtToken == null)
                {
                    return (false, "Token inválido", null);
                }

                
                var userId = int.Parse(principal.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var user = await _context.Users.FindAsync(userId);

                if (user == null)
                {
                    return (false, "Usuário não encontrado", null);
                }

                
                var newToken = GenerateJwtToken(user);

                return (true, "Token atualizado com sucesso", newToken);
            }
            catch (Exception ex)
            {
                return (false, $"Token inválido ou expirado: {ex.Message}", null);
            }
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            return HashPassword(password) == hashedPassword;
        }

        private static UserResponseDTO MapToResponseDTO(User user)
        {
            return new UserResponseDTO
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                IsMaster = user.IsMaster
            };
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtConfig.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Email),
                    new Claim(ClaimTypes.Role, user.IsMaster ? "Master" : "User"),
                    new Claim("IsMaster", user.IsMaster.ToString())
                }),
                Expires = DateTime.UtcNow.AddMinutes(_jwtConfig.ExpirationInMinutes),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _jwtConfig.Issuer,
                Audience = _jwtConfig.Audience
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
} 