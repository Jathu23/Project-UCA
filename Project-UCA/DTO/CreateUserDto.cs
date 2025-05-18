namespace Project_UCA.DTO
{
    public class CreateUserDto
    {
        public string EmployeeId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public int? PositionId { get; set; }
        public List<int>? PermissionIds { get; set; }
    }
}
