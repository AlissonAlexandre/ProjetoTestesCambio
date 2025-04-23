using System.ComponentModel.DataAnnotations;
using CambioAPI.Models;

namespace CambioAPI.DTOs
{
    public class CreateCustomerDTO
    {
        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(80, ErrorMessage = "O nome deve ter no máximo 80 caracteres")]
        public string Name { get; set; }

        [Required(ErrorMessage = "O documento é obrigatório")]
        [StringLength(14, ErrorMessage = "O documento deve ter no máximo 14 caracteres")]
        public string Document { get; set; }

        [Required(ErrorMessage = "O tipo de documento é obrigatório")]
        public DocumentType DocumentType { get; set; }

        [Required(ErrorMessage = "O telefone é obrigatório")]
        [StringLength(15, ErrorMessage = "O telefone deve ter no máximo 15 caracteres")]
        [RegularExpression(@"^\(\d{2}\)(\d{4,5})-\d{4}$", ErrorMessage = "Telefone inválido. Use o formato (XX)XXXXX-XXXX ou (XX)XXXX-XXXX")]
        public string Phone { get; set; }
    }

    public class CustomerResponseDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Document { get; set; }
        public DocumentType DocumentType { get; set; }
        public string Phone { get; set; }
        public decimal? Limit { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Email { get; set; }

    }

    public class CustomerSearchDTO
    {
        public string SearchTerm { get; set; } 
    }

    public class UpdateCustomerDTO
    {
        [StringLength(80, ErrorMessage = "O nome deve ter no máximo 80 caracteres")]
        public string Name { get; set; }

        [StringLength(15, ErrorMessage = "O telefone deve ter no máximo 15 caracteres")]
        [RegularExpression(@"^\(\d{2}\)(\d{4,5})-\d{4}$", ErrorMessage = "Telefone inválido. Use o formato (XX)XXXXX-XXXX ou (XX)XXXX-XXXX")]
        public string Phone { get; set; }
    }

    public class PaginatedResponse<T>
    {
        public List<T> Items { get; set; }
        public int TotalItems { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
    }
} 