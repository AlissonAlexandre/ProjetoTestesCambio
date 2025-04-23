using System.ComponentModel.DataAnnotations;

namespace CambioAPI.DTOs
{
    public class UserRegistrationDTO
    {
        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(80, ErrorMessage = "O nome deve ter no máximo 80 caracteres")]
        public string Name { get; set; }

        [Required(ErrorMessage = "O e-mail é obrigatório")]
        [StringLength(50, ErrorMessage = "O e-mail deve ter no máximo 50 caracteres")]
        [EmailAddress(ErrorMessage = "E-mail inválido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "A senha é obrigatória")]
        [StringLength(20, MinimumLength = 6, ErrorMessage = "A senha deve ter entre 6 e 20 caracteres")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d@$!%*?&]{6,20}$",
            ErrorMessage = "A senha deve conter letras, números e pode incluir símbolos especiais (@, $, !, %, *, ?, &), com 6 a 20 caracteres.")]
        public string Password { get; set; }
    }

    public class UserLoginDTO
    {
        [Required(ErrorMessage = "O e-mail é obrigatório")]
        [EmailAddress(ErrorMessage = "E-mail inválido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "A senha é obrigatória")]
        public string Password { get; set; }
    }

    public class UserResponseDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public bool IsMaster { get; set; }
    }

    public class LoginRequestDTO
    {
        [Required(ErrorMessage = "O email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "A senha é obrigatória")]
        public string Password { get; set; }
    }

    public class AuthResponseDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Token { get; set; }
        public UserResponseDTO User { get; set; }
    }

    public class UpdateUserRoleDTO
    {
        [Required(ErrorMessage = "O ID do usuário é obrigatório")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "A definição de master é obrigatória")]
        public bool IsMaster { get; set; }
    }
} 