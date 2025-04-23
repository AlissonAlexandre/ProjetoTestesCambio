using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace CambioAPI.Models
{
    public class Customer
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(80)]
        public string Name { get; set; }

        [Required]
        [StringLength(14)]
        public string Document { get; set; } 

        [Required]
        public DocumentType DocumentType { get; set; }

        [Required]
        [StringLength(15)]
        public string Phone { get; set; }

        [Required]
        [StringLength(50)]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public bool IsCompany { get; set; } 

        public DateTime CreatedAt { get; set; }
        
        public int CreatedByUserId { get; set; }
        public User CreatedByUser { get; set; }

        
        public CustomerLimit CustomerLimit { get; set; }
        public ICollection<ExchangeOperation> ExchangeOperations { get; set; }

        public bool IsValidDocument()
        {
            return DocumentType switch
            {
                DocumentType.CPF => ValidateCPF(Document),
                DocumentType.CNPJ => ValidateCNPJ(Document),
                _ => false
            };
        }

        public bool IsValidPhone()
        {
            if (string.IsNullOrEmpty(Phone)) return false;
            
            
            string pattern = @"^\(\d{2}\)(\d{4,5})-\d{4}$";
            return Regex.IsMatch(Phone, pattern);
        }

        private bool ValidateCPF(string cpf)
        {
            
            cpf = new string(cpf.Where(char.IsDigit).ToArray());

            if (cpf.Length != 11)
                return false;

            
            if (cpf.Distinct().Count() == 1)
                return false;

            
            int sum = 0;
            for (int i = 0; i < 9; i++)
                sum += (cpf[i] - '0') * (10 - i);

            int remainder = sum % 11;
            int firstDigit = remainder < 2 ? 0 : 11 - remainder;

            if ((cpf[9] - '0') != firstDigit)
                return false;

            
            sum = 0;
            for (int i = 0; i < 10; i++)
                sum += (cpf[i] - '0') * (11 - i);

            remainder = sum % 11;
            int secondDigit = remainder < 2 ? 0 : 11 - remainder;

            return (cpf[10] - '0') == secondDigit;
        }

        private bool ValidateCNPJ(string cnpj)
        {
            
            cnpj = new string(cnpj.Where(char.IsDigit).ToArray());

            if (cnpj.Length != 14)
                return false;

            
            if (cnpj.Distinct().Count() == 1)
                return false;

            
            int[] multiplier1 = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            int sum = 0;

            for (int i = 0; i < 12; i++)
                sum += (cnpj[i] - '0') * multiplier1[i];

            int remainder = sum % 11;
            int firstDigit = remainder < 2 ? 0 : 11 - remainder;

            if ((cnpj[12] - '0') != firstDigit)
                return false;

            
            int[] multiplier2 = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            sum = 0;

            for (int i = 0; i < 13; i++)
                sum += (cnpj[i] - '0') * multiplier2[i];

            remainder = sum % 11;
            int secondDigit = remainder < 2 ? 0 : 11 - remainder;

            return (cnpj[13] - '0') == secondDigit;
        }
    }

    public enum DocumentType
    {
        CPF,
        CNPJ
    }
} 