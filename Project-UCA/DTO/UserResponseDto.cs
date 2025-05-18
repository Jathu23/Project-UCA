using System;
using System.Collections.Generic;

namespace Project_UCA.DTOs
{
    public class UserResponseDto
    {
        public int Id { get; set; }
        public string EmployeeId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public int? PositionId { get; set; }
        public string Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<string>? Permissions { get; set; } 
        public AddressDto? Address { get; set; }
        public AccountDetailsDto? AccountDetails { get; set; }
        public List<InvoiceHistoryDto>? InvoiceHistory { get; set; }
        public InvoiceDataDto? InvoiceData { get; set; }
    }


    public class AddressDto { public int Id { get; set; } public int UserId { get; set; } public string AddressLine1 { get; set; } public string AddressLine2 { get; set; } public string City { get; set; } public string State { get; set; } public string PostalCode { get; set; } public string Country { get; set; } }

    public class AccountDetailsDto { public int Id { get; set; } public int UserId { get; set; } public string BankName { get; set; } public string AccountNumber { get; set; } public string Branch { get; set; } public string SwiftCode { get; set; } }

    public class InvoiceHistoryDto { public int Id { get; set; } public int UserId { get; set; } public int InvoiceDataId { get; set; } public string Action { get; set; } public DateTime Timestamp { get; set; } public string Details { get; set; } }

    public class InvoiceDataDto { public int Id { get; set; } public int UserId { get; set; } public string Description { get; set; } public decimal Rate { get; set; } public decimal GrossTotal { get; set; } public string InvoiceNumber { get; set; }  }
}