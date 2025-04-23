using Microsoft.AspNetCore.Mvc;
using CambioAPI.Services;
using CambioAPI.DTOs;
using System.Threading.Tasks;

namespace CambioAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerDocumentController : ControllerBase
    {
        private readonly ICustomerDocumentService _documentService;

        public CustomerDocumentController(ICustomerDocumentService documentService)
        {
            _documentService = documentService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateDocument([FromBody] CreateCustomerDocumentDTO dto)
        {
            var result = await _documentService.CreateDocumentAsync(dto);
            if (!result.success)
            {
                return BadRequest(new { message = result.message });
            }

            return Ok(new { message = result.message, document = result.document });
        }

        [HttpGet]
        public async Task<ActionResult<PagedResponseDTO<CustomerDocumentDTO>>> GetDocuments(
            [FromQuery] int? customerId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var pagination = new PaginationDTO
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await _documentService.GetCustomerDocumentsAsync(customerId, pagination);
            return Ok(result);
        }
    }
} 