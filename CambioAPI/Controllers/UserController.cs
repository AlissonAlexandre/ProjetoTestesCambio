using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CambioAPI.Services;
using CambioAPI.DTOs;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace CambioAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var profile = await _userService.GetUserProfileAsync(userId);

            if (profile == null)
            {
                return NotFound(new { message = "Usuário não encontrado" });
            }

            return Ok(profile);
        }

        [HttpPut("password")]
        [Authorize]
        public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordDTO model)
        {
            try
            {
                _logger.LogInformation("Iniciando atualização de senha");
                
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    _logger.LogWarning("Token não contém ClaimTypes.NameIdentifier");
                    return Unauthorized(new { message = "Usuário não autenticado" });
                }

                _logger.LogInformation($"UserId do token: {userIdClaim.Value}");

                var userId = int.Parse(userIdClaim.Value);
                var result = await _userService.UpdatePasswordAsync(userId, model);

                if (!result.success)
                {
                    _logger.LogWarning($"Falha ao atualizar senha: {result.message}");
                    return BadRequest(new { message = result.message });
                }

                _logger.LogInformation("Senha atualizada com sucesso");
                return Ok(new { message = result.message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar senha");
                return BadRequest(new { message = $"Erro ao atualizar senha: {ex.Message}" });
            }
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDTO model)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var result = await _userService.UpdateProfileAsync(userId, model);

            if (!result.success)
            {
                return BadRequest(new { message = result.message });
            }

            return Ok(result.profile);
        }
    }
} 