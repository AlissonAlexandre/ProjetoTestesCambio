using CambioAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CambioAPI.Data
{
    public class CurrencyDataSeeder
    {
        private readonly ApplicationDbContext _context;

        public CurrencyDataSeeder(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {
            if (!await _context.Currencies.AnyAsync())
            {
                var currencies = new List<Currency>
                {
                    new Currency { Code = "USD", Name = "United States Dollar", ExchangeRate = 5.00m },
                    new Currency { Code = "EUR", Name = "Euro", ExchangeRate = 5.50m },
                    new Currency { Code = "GBP", Name = "British Pound Sterling", ExchangeRate = 6.30m },
                    new Currency { Code = "JPY", Name = "Japanese Yen", ExchangeRate = 0.034m },
                    new Currency { Code = "CHF", Name = "Swiss Franc", ExchangeRate = 5.70m },
                    new Currency { Code = "CAD", Name = "Canadian Dollar", ExchangeRate = 3.70m },
                    new Currency { Code = "AUD", Name = "Australian Dollar", ExchangeRate = 3.30m },
                    new Currency { Code = "CNY", Name = "Chinese Yuan", ExchangeRate = 0.70m },
                    new Currency { Code = "NZD", Name = "New Zealand Dollar", ExchangeRate = 3.05m },
                    new Currency { Code = "SGD", Name = "Singapore Dollar", ExchangeRate = 3.75m },
                    new Currency { Code = "HKD", Name = "Hong Kong Dollar", ExchangeRate = 0.64m },
                    new Currency { Code = "SEK", Name = "Swedish Krona", ExchangeRate = 0.48m },
                    new Currency { Code = "KRW", Name = "South Korean Won", ExchangeRate = 0.0038m },
                    new Currency { Code = "INR", Name = "Indian Rupee", ExchangeRate = 0.060m },
                    new Currency { Code = "MXN", Name = "Mexican Peso", ExchangeRate = 0.30m },
                    new Currency { Code = "ARS", Name = "Argentine Peso", ExchangeRate = 0.0060m },
                    new Currency { Code = "DKK", Name = "Danish Krone", ExchangeRate = 0.74m },
                    new Currency { Code = "ILS", Name = "Israeli New Shekel", ExchangeRate = 1.37m },
                    new Currency { Code = "NOK", Name = "Norwegian Krone", ExchangeRate = 0.48m }
                };

                await _context.Currencies.AddRangeAsync(currencies);
                await _context.SaveChangesAsync();
            }
        }
    }
} 