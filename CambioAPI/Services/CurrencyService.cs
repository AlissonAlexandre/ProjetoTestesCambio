using System;
using System.Threading.Tasks;
using CambioAPI.Data;
using CambioAPI.DTOs;
using CambioAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CambioAPI.Services
{
    public class CurrencyService : ICurrencyService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthService _authService;

        public CurrencyService(ApplicationDbContext context, IAuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        public async Task<(bool success, string message, List<Currency> currencies)> GetAvailableCurrenciesAsync()
        {
            var currencies = await _context.Currencies.ToListAsync();
            return (true, null, currencies);
        }

        public async Task<(bool success, string message, decimal rate)> GetQuoteAsync(int fromCurrencyId, int toCurrencyId)
        {
            var fromCurrency = await _context.Currencies.FindAsync(fromCurrencyId);
            if (fromCurrency == null)
            {
                return (false, "Moeda de origem não encontrada", 0);
            }

            var toCurrency = await _context.Currencies.FindAsync(toCurrencyId);
            if (toCurrency == null)
            {
                return (false, "Moeda de destino não encontrada", 0);
            }

            
            if (fromCurrency.Code == "BRL" && toCurrency.Code == "BRL")
            {
                return (true, null, 1);
            }

            
            if (fromCurrency.Code == "BRL")
            {
                return (true, null, toCurrency.ExchangeRate);
            }

            
            if (toCurrency.Code == "BRL")
            {
                return (true, null, 1 / fromCurrency.ExchangeRate);
            }

            
            var rate = toCurrency.ExchangeRate * (1 / fromCurrency.ExchangeRate);
            return (true, null, rate);
        }

        public async Task<(bool success, string message, Currency currency)> CreateCurrencyAsync(string code, string name, decimal exchangeRate)
        {
            if (await CurrencyExistsByCodeAsync(code))
            {
                return (false, $"Já existe uma moeda com o código {code}", null);
            }

            var currency = new Currency
            {
                Code = code.ToUpper(),
                Name = name,
                ExchangeRate = exchangeRate,
                LastUpdate = DateTime.UtcNow
            };

            await _context.Currencies.AddAsync(currency);
            await _context.SaveChangesAsync();

            return (true, "Moeda criada com sucesso", currency);
        }

        public async Task<(bool success, string message)> UpdateExchangeRateAsync(int currencyId, decimal newRate)
        {
            var currency = await _context.Currencies.FindAsync(currencyId);
            if (currency == null)
            {
                return (false, "Moeda não encontrada");
            }

            currency.ExchangeRate = newRate;
            currency.LastUpdate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return (true, "Taxa de câmbio atualizada com sucesso");
        }

        public async Task<bool> CurrencyExistsAsync(int currencyId)
        {
            return await _context.Currencies.AnyAsync(c => c.Id == currencyId);
        }

        public async Task<bool> CurrencyExistsByCodeAsync(string code)
        {
            return await _context.Currencies.AnyAsync(c => c.Code == code.ToUpper());
        }

        public async Task<(bool success, string message, Currency currency)> GetCurrencyByIdAsync(int id)
        {
            var currency = await _context.Currencies.FindAsync(id);
            if (currency == null)
            {
                return (false, $"Moeda {id} não encontrada", null);
            }

            return (true, null, currency);
        }

        public async Task<(bool success, string message, Currency currency)> GetCurrencyByCodeAsync(string code)
        {
                var currency = await _context.Currencies
                .FirstOrDefaultAsync(c => c.Code == code.ToUpper());
            if (currency == null)
            {
                return (false, $"Moeda {code} não encontrada", null);
            }

            return (true, null, currency);
        }

        public async Task<List<CurrencyResponseDTO>> GetAllCurrenciesAsync()
        {
            var currencies = await _context.Currencies.ToListAsync();
            return currencies.Select(c => new CurrencyResponseDTO
            {
                Id = c.Id,
                Code = c.Code,
                Name = c.Name,
                ExchangeRate = c.ExchangeRate
            }).ToList();
        }

        public async Task<ExchangeRateResponseDTO> GetExchangeRateAsync(string fromCurrencyCode, string toCurrencyCode)
        {
            var fromCurrency = await _context.Currencies
                .FirstOrDefaultAsync(c => c.Code == fromCurrencyCode.ToUpper());
            var toCurrency = await _context.Currencies
                .FirstOrDefaultAsync(c => c.Code == toCurrencyCode.ToUpper());

            if (fromCurrency == null || toCurrency == null)
                return null;

            decimal rate;
            if (fromCurrencyCode.ToUpper() == "BRL" && toCurrencyCode.ToUpper() == "BRL")
            {
                rate = 1;
            }
            else if (fromCurrencyCode.ToUpper() == "BRL")
            {
                rate = toCurrency.ExchangeRate;
            }
            else if (toCurrencyCode.ToUpper() == "BRL")
            {
                rate = 1 / fromCurrency.ExchangeRate;
            }
            else
            {
                
                rate = toCurrency.ExchangeRate * (1 / fromCurrency.ExchangeRate);
            }

            return new ExchangeRateResponseDTO
            {
                FromCurrency = fromCurrency.Code,
                ToCurrency = toCurrency.Code,
                Rate = rate,
                LastUpdate = DateTime.UtcNow
            };
        }

        public async Task<decimal> CalculateExchangeAmountAsync(string fromCurrencyCode, string toCurrencyCode, decimal amount)
        {
            var rate = await GetExchangeRateAsync(fromCurrencyCode, toCurrencyCode);
            return rate != null ? amount * rate.Rate : 0;
        }

        public async Task<ExchangeRatesResponseDTO> GetAllExchangeRatesAsync()
        {
            
            var currencies = await _context.Currencies
                .AsNoTracking()
                .OrderBy(c => c.Id)
                .ToListAsync();

            var rates = new List<ExchangeRateDTO>();
            var brlCurrency = currencies.FirstOrDefault(c => c.Code == "BRL");

            if (brlCurrency == null)
            {
                return new ExchangeRatesResponseDTO { Rates = rates };
            }

            
            foreach (var currency in currencies)
            {
                if (currency.Code != "BRL")
                {
                    rates.Add(new ExchangeRateDTO
                    {
                        FromCurrencyId = brlCurrency.Id,
                        FromCurrencyCode = brlCurrency.Code,
                        FromCurrencyName = brlCurrency.Name,
                        ToCurrencyId = currency.Id,
                        ToCurrencyCode = currency.Code,
                        ToCurrencyName = currency.Name,
                        Rate = currency.ExchangeRate
                    });
                }
            }

            return new ExchangeRatesResponseDTO
            {
                Rates = rates
            };
        }
    }
} 