using System.ComponentModel.DataAnnotations;

namespace Project_UCA.DTOs
{
    public class AssignPermissionDto
    {
        [Required(ErrorMessage = "User ID is required")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Permission name is required")]
        public string PermissionName { get; set; }
    }
}