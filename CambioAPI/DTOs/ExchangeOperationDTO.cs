using System;
using System.ComponentModel.DataAnnotations;
using CambioAPI.Models;
using CambioAPI.Enums;

namespace CambioAPI.DTOs
{
    public class CreateExchangeOperationDTO
    {
        [Required(ErrorMessage = "O ID do cliente é obrigatório")]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "A moeda de origem é obrigatória")]
        public string FromCurrencyCode { get; set; }

        [Required(ErrorMessage = "A moeda de destino é obrigatória")]
        public string ToCurrencyCode { get; set; }

        [Required(ErrorMessage = "O valor é obrigatório")]
        [Range(0.0001, double.MaxValue, ErrorMessage = "O valor deve ser maior que zero")]
        public decimal Amount { get; set; }
    }

    public class ExchangeOperationResponseDTO
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerDocument { get; set; }
        public string FromCurrency { get; set; }
        public string ToCurrency { get; set; }
        public decimal Amount { get; set; }
        public decimal ExchangeRate { get; set; }
        public decimal FinalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedByUserName { get; set; }
        public OperationStatus Status { get; set; }
    }

    public class UpdateExchangeOperationStatusDTO
    {
        [Required(ErrorMessage = "O ID da operação é obrigatório")]
        public int OperationId { get; set; }

        [Required(ErrorMessage = "O status é obrigatório")]
        public OperationStatus NewStatus { get; set; }
    }

    public class ExchangeOperationDTO
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public int FromCurrencyId { get; set; }
        public string FromCurrencyCode { get; set; }
        public int ToCurrencyId { get; set; }
        public string ToCurrencyCode { get; set; }
        public decimal Amount { get; set; }
        public decimal ExchangeRate { get; set; }
        public decimal FinalAmount { get; set; }
        public OperationStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CreatedByUserId { get; set; }
        public string CreatedByUserName { get; set; }
    }
} 