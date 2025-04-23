using System;

namespace CambioAPI.DTOs
{
    public class CustomerDocumentDTO
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string Document { get; set; }  
        public DateTime CreatedAt { get; set; }
    }

    public class CreateCustomerDocumentDTO
    {
        public int CustomerId { get; set; }
        public string Document { get; set; }  
    }
} 