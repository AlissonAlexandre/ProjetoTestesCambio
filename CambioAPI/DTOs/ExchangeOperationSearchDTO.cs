using System;
using System.Collections.Generic;
using CambioAPI.Enums;

namespace CambioAPI.DTOs
{
    public class ExchangeOperationSearchDTO
    {
        private int _pageSize = 10;
        private const int MaxPageSize = 50;

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? CustomerId { get; set; }
        public OperationStatus? Status { get; set; }
        public string SortBy { get; set; } = "createdAt";
        public bool Ascending { get; set; } = false;
        public int PageNumber { get; set; } = 1;
        
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
        }
    }

    public class ExchangeOperationSearchResponseDTO
    {
        public List<ExchangeOperationDTO> Operations { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
        public decimal TotalAmount { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
    }
} 