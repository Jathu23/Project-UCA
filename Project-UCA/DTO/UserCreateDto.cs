using System.ComponentModel.DataAnnotations;

namespace Project_UCA.DTOs
{
    public class UserCreateDto
    {
        [Required(ErrorMessage = "Employee ID is required")]
        [StringLength(50, ErrorMessage = "Employee ID cannot exceed 50 characters")]
        public string EmployeeId { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [StringLength(100, ErrorMessage = "First name cannot exceed 100 characters")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(100, ErrorMessage = "Last name cannot exceed 100 characters")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters")]
        public string Password { get; set; }

        public int? PositionId { get; set; }

        [Required(ErrorMessage = "Role is required")]
        [RegularExpression("^(Master|Admin|User)$", ErrorMessage = "Role must be Master, Admin, or User")]
        public string Role { get; set; }
    }
}