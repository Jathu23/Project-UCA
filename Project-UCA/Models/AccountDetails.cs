using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project_UCA.Models
{
    public class AccountDetails
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int UserId { get; set; }
        [Required]
        public string BankName { get; set; }
        [Required]
        public string AccountNumber { get; set; }
        [Required]
        public string Branch { get; set; }
        [Required]
        public string SwiftCode { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
    }
}