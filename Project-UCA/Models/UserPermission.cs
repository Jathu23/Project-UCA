using System.ComponentModel.DataAnnotations.Schema;

namespace Project_UCA.Models
{
    public class UserPermission
    {
        public int UserId { get; set; }
        public int PermissionId { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
        [ForeignKey("PermissionId")]
        public Permission Permission { get; set; }
    }



}
