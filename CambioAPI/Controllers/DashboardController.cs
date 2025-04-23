using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CambioAPI.Data;
using CambioAPI.DTOs;

namespace CambioAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("stats")]
        public async Task<ActionResult<DashboardStatsDTO>> GetStats()
        {
            var stats = new DashboardStatsDTO
            {
                TotalCustomers = await _context.Customers.CountAsync(),
                TotalOperations = await _context.ExchangeOperations.CountAsync(),
                TotalExchangeVolume = await _context.ExchangeOperations.SumAsync(op => op.FinalAmount)
            };

            return Ok(stats);
        }
    }
} 