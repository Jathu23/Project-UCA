using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;

namespace Project_UCA.Models
{
    public class ApplicationUser : IdentityUser<int>
    {
        [Required]
        public string EmployeeId { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        public string? ProfileImage { get; set; }
        public string? Signature { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public int? PositionId { get; set; }

        // Navigation Properties
        [JsonIgnore]
        public Address? Address { get; set; }
        [JsonIgnore]
        public AccountDetails? AccountDetails { get; set; }
        [JsonIgnore]
        public InvoiceData? InvoiceData { get; set; }
        [JsonIgnore]
        public ICollection<InvoiceHistory> InvoiceHistories { get; set; } = new List<InvoiceHistory>();
        [JsonIgnore]
        [ForeignKey("PositionId")]
        public Position? Position { get; set; }
        [JsonIgnore]
        public ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
        [JsonIgnore]
        public ICollection<SignupRequest> ApprovedSignupRequests { get; set; } = new List<SignupRequest>();
    }
}