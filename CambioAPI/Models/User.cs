using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace CambioAPI.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(80, ErrorMessage = "O nome deve ter no máximo 80 caracteres")]
        public string Name { get; set; }

        [Required(ErrorMessage = "O email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        [StringLength(50, ErrorMessage = "O email deve ter no máximo 50 caracteres")]
        public string Email { get; set; }

        [Required(ErrorMessage = "A senha é obrigatória")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "A senha deve ter entre 6 e 20 caracteres")]
        public string Password { get; set; }

        public bool IsMaster { get; set; }

        public DateTime CreatedAt { get; set; }

        
        public bool IsValidPassword(string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length > 20)
                return false;

            
            var hasLetter = Regex.IsMatch(password, "[a-zA-Z]");
            var hasNumber = Regex.IsMatch(password, @"\d");

            return hasLetter && hasNumber;
        }
    }
} 