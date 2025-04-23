using System.ComponentModel.DataAnnotations;

namespace CambioAPI.DTOs
{
    public class CreateCustomerLimitDTO
    {
        [Required(ErrorMessage = "O ID do cliente é obrigatório")]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "O limite é obrigatório")]
        [Range(0, double.MaxValue, ErrorMessage = "O limite deve ser maior ou igual a zero")]
        public decimal Limit { get; set; }
    }

    public class UpdateCustomerLimitDTO
    {
        [Required(ErrorMessage = "O ID do limite é obrigatório")]
        public int LimitId { get; set; }

        [Required(ErrorMessage = "O novo limite é obrigatório")]
        [Range(0, double.MaxValue, ErrorMessage = "O limite deve ser maior ou igual a zero")]
        public decimal NewLimit { get; set; }
    }

    public class CustomerLimitResponseDTO
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerDocument { get; set; }
        public decimal Limit { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public string CreatedByUserName { get; set; }
        public string LastUpdatedByUserName { get; set; }
    }
} 