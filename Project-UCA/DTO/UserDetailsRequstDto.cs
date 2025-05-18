using System.ComponentModel.DataAnnotations;

namespace Project_UCA.DTOs
{
    public class AccountDetailsDto_Request
    {
        [Required(ErrorMessage = "User ID is required")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Bank name is required")]
        public string BankName { get; set; }

        [Required(ErrorMessage = "Account number is required")]
        public string AccountNumber { get; set; }

        [Required(ErrorMessage = "Branch is required")]
        public string Branch { get; set; }

        [Required(ErrorMessage = "Swift code is required")]
        public string SwiftCode { get; set; }
    }
    public class AddressDto_Request
    {
        [Required(ErrorMessage = "User ID is required")] 
        public int UserId { get; set; }

        public string AddressLine1 { get; set; }

        public string AddressLine2 { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string PostalCode { get; set; }

        public string Country { get; set; }
    }
    public class InvoiceDataDto_Request
    {
        [Required(ErrorMessage = "User ID is required")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Invoice number is required")]
        public string InvoiceNumber { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Rate is required")]
        public decimal Rate { get; set; }

        [Required(ErrorMessage = "Gross total is required")]
        public decimal GrossTotal { get; set; }
    }
}