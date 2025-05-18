using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project_UCA.Models
{
    public class SignupRequest
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string EmployeeId { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
        [Required]
        public DateTime RequestDate { get; set; }
        public int? ApprovedBy { get; set; }
        public DateTime? ApprovedOn { get; set; }
        [Required]
        public SignupStatus Status { get; set; } = SignupStatus.Pending;

        [ForeignKey("ApprovedBy")]
        public ApplicationUser? ApprovedByUser { get; set; }
    }

    public enum SignupStatus
    {
        Pending,
        Approved,
        Rejected
    }
}