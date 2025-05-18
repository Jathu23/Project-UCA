using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Project_UCA.Models
{
    public class Position
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string? Description { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [JsonIgnore]
        public ICollection<PositionPermission> PositionPermissions { get; set; } = new List<PositionPermission>();
        [JsonIgnore]
        public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    }
}