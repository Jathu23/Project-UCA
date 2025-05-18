using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Project_UCA.Models
{
    public class Permission
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string? Description { get; set; }

        [JsonIgnore]
        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
        [JsonIgnore]
        public ICollection<PositionPermission> PositionPermissions { get; set; } = new List<PositionPermission>();
        [JsonIgnore]
        public ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
    }
}