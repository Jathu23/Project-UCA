using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Project_UCA.DTO
{
    public class CreatePositionDto 
    { [Required(ErrorMessage = "Position name is required")]
      public string Name { get; set; }
      public string? Description { get; set; }
      public List<int> PermissionIds { get; set; }
    }

    public class UpdatePositionDto
    {
        [Required(ErrorMessage = "Position name is required")]
        public string Name { get; set; }
        public string? Description { get; set; }
    }
    public class UpdateUserPositionDto
    {
        [Required(ErrorMessage = "User ID is required")]
        public int UserId { get; set; }
        [Required(ErrorMessage = "Position ID is required")]
        public int PositionId { get; set; }
    }

    public class PositionResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public List<int> PermissionIds { get; set; }
        public List<PermissionDto> Permissions { get; set; }
    }

    public class PermissionDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
    }
}
