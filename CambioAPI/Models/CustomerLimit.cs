using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CambioAPI.Models
{
    public class CustomerLimit
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [Required]
        [Precision(18, 2)]
        [Range(0, double.MaxValue, ErrorMessage = "O limite deve ser maior ou igual a zero")]
        public decimal Limit { get; set; }

        [ForeignKey(nameof(CustomerId))]
        public Customer Customer { get; set; }

        [Required]
        public int CreatedByUserId { get; set; }
        public User CreatedByUser { get; set; }

        public DateTime CreatedAt { get; set; }

        public int? LastUpdatedByUserId { get; set; }
        public User LastUpdatedByUser { get; set; }

        public DateTime? LastUpdatedAt { get; set; }
    }
} 