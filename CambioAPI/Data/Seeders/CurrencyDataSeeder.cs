using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CambioAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CambioAPI.Data.Seeders
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
                    new Currency { Code = "BRL", Name = "Real", ExchangeRate = 1.0m },
                    new Currency { Code = "USD", Name = "Dólar Americano", ExchangeRate = 5.0m },
                    new Currency { Code = "EUR", Name = "Euro", ExchangeRate = 5.5m },
                    new Currency { Code = "GBP", Name = "Libra Esterlina", ExchangeRate = 6.3m },
                    new Currency { Code = "JPY", Name = "Iene Japonês", ExchangeRate = 0.034m },
                    new Currency { Code = "CHF", Name = "Franco Suíço", ExchangeRate = 5.7m },
                    new Currency { Code = "CAD", Name = "Dólar Canadense", ExchangeRate = 3.7m },
                    new Currency { Code = "AUD", Name = "Dólar Australiano", ExchangeRate = 3.3m },
                    new Currency { Code = "CNY", Name = "Yuan Chinês", ExchangeRate = 0.7m },
                    new Currency { Code = "NZD", Name = "Dólar Neozelandês", ExchangeRate = 3.05m },
                    new Currency { Code = "SGD", Name = "Dólar de Singapura", ExchangeRate = 3.75m },
                    new Currency { Code = "HKD", Name = "Dólar de Hong Kong", ExchangeRate = 0.64m },
                    new Currency { Code = "SEK", Name = "Coroa Sueca", ExchangeRate = 0.48m },
                    new Currency { Code = "KRW", Name = "Won Sul-Coreano", ExchangeRate = 0.0038m },
                    new Currency { Code = "INR", Name = "Rúpia Indiana", ExchangeRate = 0.060m },
                    new Currency { Code = "MXN", Name = "Peso Mexicano", ExchangeRate = 0.30m },
                    new Currency { Code = "ARS", Name = "Peso Argentino", ExchangeRate = 0.0060m },
                    new Currency { Code = "DKK", Name = "Coroa Dinamarquesa", ExchangeRate = 0.74m },
                    new Currency { Code = "ILS", Name = "Novo Shekel Israelense", ExchangeRate = 1.37m },
                    new Currency { Code = "NOK", Name = "Coroa Norueguesa", ExchangeRate = 0.48m }
                };

                await _context.Currencies.AddRangeAsync(currencies);
                await _context.SaveChangesAsync();
            }
        }
    }
} 