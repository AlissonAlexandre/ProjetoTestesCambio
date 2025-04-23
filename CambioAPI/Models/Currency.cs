using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace CambioAPI.Models
{
    public class Currency
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(3)]
        public string Code { get; set; } 

        [Required]
        [StringLength(50)]
        public string Name { get; set; } 

        [Required]
        [Range(0, double.MaxValue)]
        [Precision(18, 4)]
        public decimal ExchangeRate { get; set; } 

        public DateTime LastUpdate { get; set; }

        
        public virtual ICollection<ExchangeOperation> FromOperations { get; set; }
        public virtual ICollection<ExchangeOperation> ToOperations { get; set; }
    }
} 