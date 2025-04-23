using System.Threading.Tasks;
using System.Collections.Generic;
using CambioAPI.DTOs;
using CambioAPI.Models;

namespace CambioAPI.Services
{
    public interface ICurrencyService
    {
        Task<(bool success, string message, List<Currency> currencies)> GetAvailableCurrenciesAsync();
        Task<(bool success, string message, decimal rate)> GetQuoteAsync(int fromCurrencyId, int toCurrencyId);
        Task<(bool success, string message, Currency currency)> CreateCurrencyAsync(string code, string name, decimal exchangeRate);
        Task<(bool success, string message)> UpdateExchangeRateAsync(int currencyId, decimal newRate);
        Task<bool> CurrencyExistsAsync(int currencyId);
        Task<bool> CurrencyExistsByCodeAsync(string code);
        Task<(bool success, string message, Currency currency)> GetCurrencyByIdAsync(int id);
        Task<(bool success, string message, Currency currency)> GetCurrencyByCodeAsync(string code);
        Task<List<CurrencyResponseDTO>> GetAllCurrenciesAsync();
        Task<ExchangeRateResponseDTO> GetExchangeRateAsync(string fromCurrencyCode, string toCurrencyCode);
        Task<decimal> CalculateExchangeAmountAsync(string fromCurrencyCode, string toCurrencyCode, decimal amount);
        Task<ExchangeRatesResponseDTO> GetAllExchangeRatesAsync();
    }
} 