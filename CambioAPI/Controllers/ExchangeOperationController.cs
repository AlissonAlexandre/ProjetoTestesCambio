using Microsoft.AspNetCore.Mvc;
using CambioAPI.Services;
using CambioAPI.DTOs;
using CambioAPI.Enums;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System;

namespace CambioAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExchangeOperationController : ControllerBase
    {
        private readonly IExchangeOperationService _exchangeOperationService;

        public ExchangeOperationController(IExchangeOperationService exchangeOperationService)
        {
            _exchangeOperationService = exchangeOperationService;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateOperation([FromBody] CreateExchangeOperationDTO model)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var result = await _exchangeOperationService.CreateOperationAsync(model, currentUserId);

                if (!result.success)
                {
                    return BadRequest(new { 
                        success = false,
                        message = result.message,
                        details = result.message
                    });
                }

                return Ok(new { 
                    success = true,
                    message = result.message,
                    operation = result.operation
                });
            }
            catch (FormatException)
            {
                return BadRequest(new { 
                    success = false,
                    message = "Token de autenticação inválido",
                    details = "O token fornecido não contém um ID de usuário válido"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    success = false,
                    message = "Erro interno ao processar a operação de câmbio",
                    details = ex.Message
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOperation(int id)
        {
            var operation = await _exchangeOperationService.GetOperationByIdAsync(id);
            if (operation == null)
                return NotFound($"Operação {id} não encontrada");

            return Ok(operation);
        }

        [HttpGet("search")]
        public async Task<ActionResult<ExchangeOperationSearchResponseDTO>> SearchOperations(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] int? customerId,
            [FromQuery] OperationStatus? status,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var searchParams = new ExchangeOperationSearchDTO
            {
                StartDate = startDate,
                EndDate = endDate,
                CustomerId = customerId,
                Status = status,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await _exchangeOperationService.SearchOperationsAsync(searchParams);
            return Ok(result);
        }

        [HttpGet("{operationId}/ticket")]
        [Authorize]
        public async Task<IActionResult> GenerateTicket(int operationId)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var result = await _exchangeOperationService.GenerateTicketAsync(operationId, currentUserId);

            if (!result.success)
                return BadRequest(result.message);

            return Ok(new { ticket = result.ticket });
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateOperation(int id, [FromBody] UpdateExchangeOperationDTO model)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var result = await _exchangeOperationService.UpdateOperationAsync(id, model, currentUserId);

                if (!result.success)
                {
                    return BadRequest(new { 
                        success = false,
                        message = result.message,
                        details = result.message
                    });
                }

                return Ok(new { 
                    success = true,
                    message = result.message,
                    operation = result.operation
                });
            }
            catch (FormatException)
            {
                return BadRequest(new { 
                    success = false,
                    message = "Token de autenticação inválido",
                    details = "O token fornecido não contém um ID de usuário válido"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    success = false,
                    message = "Erro interno ao processar a atualização da operação",
                    details = ex.Message
                });
            }
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteOperation(int id)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var result = await _exchangeOperationService.DeleteOperationAsync(id, currentUserId);

                if (!result.success)
                {
                    return BadRequest(new { 
                        success = false,
                        message = result.message
                    });
                }

                return Ok(new { 
                    success = true,
                    message = result.message
                });
            }
            catch (FormatException)
            {
                return BadRequest(new { 
                    success = false,
                    message = "Token de autenticação inválido",
                    details = "O token fornecido não contém um ID de usuário válido"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    success = false,
                    message = "Erro interno ao excluir a operação",
                    details = ex.Message
                });
            }
        }

        [HttpGet("paged")]
        public async Task<ActionResult<ExchangeOperationSearchResponseDTO>> GetPagedOperations(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] int? customerId,
            [FromQuery] OperationStatus? status,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string sortBy = "createdAt",
            [FromQuery] bool ascending = false)
        {
            var searchParams = new ExchangeOperationSearchDTO
            {
                StartDate = startDate,
                EndDate = endDate,
                CustomerId = customerId,
                Status = status,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortBy = sortBy,
                Ascending = ascending
            };

            var result = await _exchangeOperationService.GetPagedOperationsAsync(searchParams);
            return Ok(result);
        }
    }
} 