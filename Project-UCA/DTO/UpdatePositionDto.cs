using System.ComponentModel.DataAnnotations;

namespace Project_UCA.DTOs
{
    public class UpdatePositionDto
    {
        [Required(ErrorMessage = "User ID is required")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Position ID is required")]
        public int PositionId { get; set; }
    }
}