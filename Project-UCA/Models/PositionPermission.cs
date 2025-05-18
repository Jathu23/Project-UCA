using System.ComponentModel.DataAnnotations.Schema;

namespace Project_UCA.Models
{
    public class PositionPermission
    {
        public int PositionId { get; set; }
        public int PermissionId { get; set; }

        [ForeignKey("PositionId")]
        public Position Position { get; set; }
        [ForeignKey("PermissionId")]
        public Permission Permission { get; set; }
    }
}
