using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CambioAPI.Services;
using CambioAPI.DTOs;
using System.Security.Claims;

namespace CambioAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] UserRegistrationDTO model)
        {
            var result = await _authService.RegisterAsync(model);
            if (!result.success)
            {
                return BadRequest(new { message = result.message });
            }

            return Ok(new { 
                message = result.message,
                user = result.user,
                token = result.token
            });
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] UserLoginDTO model)
        {
            var result = await _authService.LoginAsync(model);
            if (!result.success)
            {
                return BadRequest(new { message = result.message });
            }

            return Ok(new { 
                message = result.message,
                user = result.user,
                token = result.token
            });
        }

        [HttpGet("all-users")]
        [Authorize(Roles = "Master")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _authService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpPost("update-role")]
        [Authorize]
        public async Task<IActionResult> UpdateRole([FromBody] UpdateUserRoleDTO model)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized(new { message = "Usuário não autenticado" });
            }

            var currentUserId = int.Parse(userIdClaim.Value);
            var result = await _authService.UpdateUserRoleAsync(model, currentUserId);
            
            if (!result.success)
            {
                return BadRequest(new { message = result.message });
            }

            return Ok(new { message = result.message });
        }

        [HttpPost("create-first-master")]
        [AllowAnonymous]
        public async Task<IActionResult> CreateFirstMaster([FromBody] UserRegistrationDTO model)
        {
            try
            {
                await _authService.CreateFirstMasterUserAsync(model);
                return Ok(new { message = "Primeiro usuário master criado com sucesso" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDTO model)
        {
            if (string.IsNullOrEmpty(model.RefreshToken))
            {
                return BadRequest(new { message = "Token de atualização é obrigatório" });
            }

            var result = await _authService.RefreshTokenAsync(model.RefreshToken);
            if (!result.success)
            {
                return BadRequest(new { message = result.message });
            }

            return Ok(new { token = result.token });
        }
    }
} 