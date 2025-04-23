using System.ComponentModel.DataAnnotations;

namespace CambioAPI.DTOs
{
    public class CustomerLimitUpdateDTO
    {
        [Required(ErrorMessage = "O limite é obrigatório")]
        [Range(0, double.MaxValue, ErrorMessage = "O limite deve ser maior ou igual a zero")]
        public decimal Limit { get; set; }
    }
} 