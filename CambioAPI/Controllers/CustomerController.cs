using Microsoft.AspNetCore.Mvc;
using CambioAPI.Services;
using CambioAPI.DTOs;
using System.Security.Claims;

namespace CambioAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateCustomer([FromBody] CustomerCreateDTO model)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Unauthorized(new { message = "Usuário não autenticado" });
                }

                int currentUserId = int.Parse(userIdClaim.Value);
                var (success, message, customer) = await _customerService.CreateCustomerAsync(model, currentUserId);

                if (!success)
                {
                    if(message.Equals("Já existe um cliente cadastrado com este documento", StringComparison.OrdinalIgnoreCase))
                        return Conflict(new { message });
                    return BadRequest(new { message });
                }
                return Ok(new { message = $"Cliente cadastrado com sucesso: {customer.Document}" }); 
                }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ocorreu um erro inesperado", details = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomer(int id, [FromBody] CustomerCreateDTO model)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Unauthorized(new { message = "Usuário não autenticado" });
                }

                var (success, message, customer) = await _customerService.UpdateCustomerAsync(id, model);

                if (!success)
                {
                    if (message.Equals("Cliente não encontrado", StringComparison.OrdinalIgnoreCase))
                        return NotFound(new { message });
                    return BadRequest(new { message });
                }

                return Ok(new { message = $"Cliente atualizado com sucesso: {customer.Document}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ocorreu um erro inesperado", details = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCustomer(int id)
        {
            var result = await _customerService.GetCustomerByIdAsync(id);
            if (!result.success)
            {
                return NotFound(new { message = result.message });
            }

            return Ok(result.customer);
        }

        [HttpGet("document/{document}")]
        public async Task<IActionResult> GetCustomerByDocument(string document)
        {
            var customerResult = await _customerService.GetCustomerByDocumentAsync(document);
            if (!customerResult.success)
            {
                return NotFound(new { message = customerResult.message });
            }

            return Ok(customerResult.customer);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchCustomers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100; 

            var result = await _customerService.GetPaginatedCustomersAsync(pageNumber, pageSize);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Unauthorized(new { message = "Usuário não autenticado" });
                }

                var (success, message) = await _customerService.DeleteCustomerAsync(id);

                if (!success)
                {
                    if (message.Equals("Cliente não encontrado", StringComparison.OrdinalIgnoreCase))
                        return NotFound(new { message });
                    return BadRequest(new { message });
                }

                return Ok(new { message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ocorreu um erro inesperado", details = ex.Message });
            }
        }
    }
} 