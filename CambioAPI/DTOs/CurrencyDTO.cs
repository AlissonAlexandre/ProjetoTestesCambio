using System.ComponentModel.DataAnnotations;

namespace CambioAPI.DTOs
{
    public class CurrencyQuoteRequestDTO
    {
        [Required]
        public string CurrencyCode { get; set; }
        
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }
    }

    public class CurrencyQuoteResponseDTO
    {
        public string FromCurrency { get; set; }
        public string ToCurrency { get; set; }
        public decimal Amount { get; set; }
        public decimal ConvertedAmount { get; set; }
        public decimal ExchangeRate { get; set; }
        public DateTime QuoteDateTime { get; set; }
    }

    public class CurrencyResponseDTO
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public decimal ExchangeRate { get; set; }
    }

    public class QuoteResponseDTO
    {
        public string FromCurrencyCode { get; set; }
        public string ToCurrencyCode { get; set; }
        public decimal ExchangeRate { get; set; }
        public DateTime QuoteDateTime { get; set; }
    }

    public class CreateCurrencyDTO
    {
        [Required(ErrorMessage = "O código da moeda é obrigatório")]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "O código da moeda deve ter exatamente 3 caracteres")]
        public string Code { get; set; }

        [Required(ErrorMessage = "O nome da moeda é obrigatório")]
        [StringLength(50, ErrorMessage = "O nome da moeda deve ter no máximo 50 caracteres")]
        public string Name { get; set; }

        [Required(ErrorMessage = "A taxa de câmbio é obrigatória")]
        [Range(0.000001, double.MaxValue, ErrorMessage = "A taxa de câmbio deve ser maior que zero")]
        public decimal ExchangeRate { get; set; }
    }

    public class UpdateCurrencyRateDTO
    {
        [Required(ErrorMessage = "O código da moeda é obrigatório")]
        public string Code { get; set; }

        [Required(ErrorMessage = "A taxa de câmbio é obrigatória")]
        [Range(0.0001, double.MaxValue, ErrorMessage = "A taxa de câmbio deve ser maior que zero")]
        public decimal ExchangeRate { get; set; }
    }

    public class ExchangeRateResponseDTO
    {
        public string FromCurrency { get; set; }
        public string ToCurrency { get; set; }
        public decimal Rate { get; set; }
        public DateTime LastUpdate { get; set; }
    }

    public class UpdateExchangeRateDTO
    {
        [Required(ErrorMessage = "A nova taxa de câmbio é obrigatória")]
        [Range(0.000001, double.MaxValue, ErrorMessage = "A taxa de câmbio deve ser maior que zero")]
        public decimal NewRate { get; set; }
    }
} 