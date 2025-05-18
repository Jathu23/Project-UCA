using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project_UCA.Models
{
    public class InvoiceData
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int UserId { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public decimal Rate { get; set; }
        [Required]
        public decimal GrossTotal { get; set; }
        [Required]
        public string InvoiceNumber { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
        public ICollection<InvoiceHistory> InvoiceHistories { get; set; } = new List<InvoiceHistory>();
    }
}