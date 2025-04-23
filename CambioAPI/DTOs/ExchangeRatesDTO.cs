using System.Collections.Generic;

namespace CambioAPI.DTOs
{
    public class ExchangeRateDTO
    {
        public int FromCurrencyId { get; set; }
        public string FromCurrencyCode { get; set; }
        public string FromCurrencyName { get; set; }
        public int ToCurrencyId { get; set; }
        public string ToCurrencyCode { get; set; }
        public string ToCurrencyName { get; set; }
        public decimal Rate { get; set; }
    }

    public class ExchangeRatesResponseDTO
    {
        public List<ExchangeRateDTO> Rates { get; set; }
    }
} 