using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project_UCA.Models
{
    public class InvoiceHistory
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int UserId { get; set; }
        [Required]
        public int InvoiceDataId { get; set; }
        [Required]
        public string Action { get; set; }
        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? Details { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
        [ForeignKey("InvoiceDataId")]
        public InvoiceData InvoiceData { get; set; }
    }
}