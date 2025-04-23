using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CambioAPI.Models
{
    public class CustomerDocument
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [ForeignKey(nameof(CustomerId))]
        public Customer Customer { get; set; }

        public string Document { get; set; }  

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
} 