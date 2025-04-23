using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using CambioAPI.Enums;

namespace CambioAPI.Models
{
    public class ExchangeOperation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }

        [Required]
        public int FromCurrencyId { get; set; }
        public Currency FromCurrency { get; set; }

        [Required]
        public int ToCurrencyId { get; set; }
        public Currency ToCurrency { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        [Precision(18, 4)]
        public decimal Amount { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        [Precision(18, 4)]
        public decimal ExchangeRate { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        [Precision(18, 4)]
        public decimal FinalAmount { get; set; }

        public DateTime CreatedAt { get; set; }

        [Required]
        public int CreatedByUserId { get; set; }
        public User CreatedByUser { get; set; }

        public OperationStatus Status { get; set; }
    }
} 