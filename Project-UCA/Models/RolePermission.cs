using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project_UCA.Models
{
    public class RolePermission
    {
        public int RoleId { get; set; } // Changed to int to match IdentityRole<int>
        public int PermissionId { get; set; }

        [ForeignKey("RoleId")]
        public IdentityRole<int> Role { get; set; }
        [ForeignKey("PermissionId")]
        public Permission Permission { get; set; }
    }

}
