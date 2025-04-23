using System.ComponentModel.DataAnnotations;

namespace CambioAPI.DTOs
{
    public class UpdateExchangeOperationDTO
    {
        [Required]
        public int CustomerId { get; set; }

        [Required]
        public string FromCurrencyCode { get; set; }

        [Required]
        public string ToCurrencyCode { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "O valor deve ser maior que zero")]
        public decimal Amount { get; set; }
    }
} 