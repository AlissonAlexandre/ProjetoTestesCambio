using System.ComponentModel.DataAnnotations;

namespace CambioAPI.DTOs
{
    public class CreateUserDTO
    {
        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(80, ErrorMessage = "O nome deve ter no máximo 80 caracteres")]
        public string Name { get; set; }

        [Required(ErrorMessage = "O email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        [StringLength(50, ErrorMessage = "O email deve ter no máximo 50 caracteres")]
        public string Email { get; set; }

        [Required(ErrorMessage = "A senha é obrigatória")]
        [StringLength(20, MinimumLength = 6, ErrorMessage = "A senha deve ter entre 6 e 20 caracteres")]
        public string Password { get; set; }

        public bool IsMaster { get; set; }
    }

    public class UserProfileDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public bool IsMaster { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class UpdatePasswordDTO
    {
        [Required(ErrorMessage = "A senha atual é obrigatória")]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "A nova senha é obrigatória")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "A senha deve ter entre 6 e 100 caracteres")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "A confirmação da nova senha é obrigatória")]
        [Compare("NewPassword", ErrorMessage = "As senhas não conferem")]
        public string ConfirmNewPassword { get; set; }
    }

    public class UpdateProfileDTO
    {
        [StringLength(80, ErrorMessage = "O nome deve ter no máximo 80 caracteres")]
        public string Name { get; set; }

        [EmailAddress(ErrorMessage = "Email inválido")]
        [StringLength(50, ErrorMessage = "O email deve ter no máximo 50 caracteres")]
        public string Email { get; set; }
    }

    public class RefreshTokenDTO
    {
        [Required(ErrorMessage = "O token de atualização é obrigatório")]
        public string RefreshToken { get; set; }
    }
} 