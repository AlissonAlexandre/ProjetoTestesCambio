using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CambioAPI.DTOs;
using CambioAPI.Services;
using System.Security.Claims;

namespace CambioAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerLimitController : ControllerBase
    {
        private readonly ICustomerLimitService _customerLimitService;
        private readonly IAuthService _authService;

        public CustomerLimitController(ICustomerLimitService customerLimitService, IAuthService authService)
        {
            _customerLimitService = customerLimitService;
            _authService = authService;
        }

        [HttpGet("{customerId}")]
        [Authorize]
        public async Task<IActionResult> GetCustomerLimits(int customerId)
        {
            var limits = await _customerLimitService.GetAllCustomerLimitsAsync(customerId);
            return Ok(limits);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateCustomerLimit(CreateCustomerLimitDTO model)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var result = await _customerLimitService.CreateCustomerLimitAsync(model, currentUserId);

            if (!result.success)
                return BadRequest(result.message);

            return Ok(result.limit);
        }

        [HttpPut]
        [Authorize]
        public async Task<IActionResult> UpdateCustomerLimit(UpdateCustomerLimitDTO model)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var result = await _customerLimitService.UpdateCustomerLimitAsync(model, currentUserId);

            if (!result.success)
                return BadRequest(result.message);

            return Ok(result.limit);
        }
    }
} 