using Microsoft.AspNetCore.Mvc;
using CambioAPI.Services;
using CambioAPI.DTOs;

namespace CambioAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CurrencyController : ControllerBase
    {
        private readonly ICurrencyService _currencyService;

        public CurrencyController(ICurrencyService currencyService)
        {
            _currencyService = currencyService;
        }

        [HttpGet("available")]
        public async Task<IActionResult> GetAvailableCurrencies()
        {
            var currencies = await _currencyService.GetAvailableCurrenciesAsync();
            return Ok(currencies);
        }

        [HttpGet("quote/{fromCurrencyId}/{toCurrencyId}")]
        public async Task<IActionResult> GetQuote(int fromCurrencyId, int toCurrencyId)
        {
            var result = await _currencyService.GetQuoteAsync(fromCurrencyId, toCurrencyId);
            if (!result.success)
            {
                return BadRequest(new { message = result.message });
            }

            return Ok(new { rate = result.rate });
        }

        [HttpGet("rates")]
        public async Task<ActionResult<ExchangeRatesResponseDTO>> GetAllRates()
        {
            var rates = await _currencyService.GetAllExchangeRatesAsync();
            return Ok(rates);
        }

        [HttpPut("rate/{currencyId}")]
        public async Task<IActionResult> UpdateExchangeRate(int currencyId, [FromBody] decimal newRate)
        {
            var result = await _currencyService.UpdateExchangeRateAsync(currencyId, newRate);
            if (!result.success)
            {
                return BadRequest(new { message = result.message });
            }

            return Ok(new { message = result.message });
        }
    }
} 