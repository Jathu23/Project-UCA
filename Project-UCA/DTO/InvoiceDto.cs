using Project_UCA.Models;

namespace Project_UCA.DTO
{
    public class InvoiceResponseDto
    {
        public string InvoiceNumber { get; set; } = string.Empty;
        public string InvoiceDate { get; set; } = string.Empty;
        public string NameWithInitials { get; set; } = string.Empty;
        public Address? Address { get; set; }
        public AccountDetails? AccountDetails { get; set; }
        public InvoiceData? InvoiceData { get; set; }
    }

    public class InvoiceHistoryDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public string InvoiceDate { get; set; } = string.Empty;
        public int InvoiceDataId { get; set; }
        public string Action { get; set; }
        public string Timestamp { get; set; }
        public string Details { get; set; }
    }
}