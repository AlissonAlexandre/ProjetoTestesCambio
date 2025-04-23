using System.ComponentModel.DataAnnotations;

namespace CambioAPI.DTOs
{
    public class CustomerCreateDTO
    {
        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(80, ErrorMessage = "O nome deve ter no máximo 80 caracteres")]
        public string Name { get; set; }

        [Required(ErrorMessage = "O documento é obrigatório")]
        [StringLength(14, ErrorMessage = "O documento deve ter no máximo 14 caracteres")]
        public string Document { get; set; }

        [Required(ErrorMessage = "O telefone é obrigatório")]
        [StringLength(15, ErrorMessage = "O telefone deve ter no máximo 15 caracteres")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "O email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        [StringLength(50, ErrorMessage = "O email deve ter no máximo 50 caracteres")]
        public string Email { get; set; }
    }
} 