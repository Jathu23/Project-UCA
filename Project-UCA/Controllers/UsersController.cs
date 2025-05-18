using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project_UCA.Data;
using Project_UCA.DTOs;
using Project_UCA.Services.Interfaces;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Project_UCA.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IPermissionService _permissionService;
        private readonly ApplicationDbContext _context;

        public UsersController(
            IUserService userService,
            IPermissionService permissionService,
            ApplicationDbContext context)
        {
            _userService = userService;
            _permissionService = permissionService;
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] UserCreateDto userDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var callerUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(callerUserIdClaim, out int callerUserId))
            {
                return Unauthorized("Invalid user ID in token.");
            }

            var (success, errorMessage) = await _userService.CreateUserAsync(userDto, callerUserId);
            if (!success)
            {
                return BadRequest(new { Error = errorMessage });
            }

            return CreatedAtAction(nameof(CreateUser), new { email = userDto.Email }, new { Message = "User created successfully." });
        }

        [HttpPost("update-position")]
        public async Task<IActionResult> UpdatePosition([FromBody] UpdatePositionDto dto)
        {
            var callerUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            if (!await _permissionService.HasPermissionAsync(callerUserId, "ManagePositions"))
            {
                return Forbid("You do not have permission to manage positions.");
            }

            var user = await _context.Users.FindAsync(dto.UserId);
            if (user == null)
            {
                return BadRequest(new { Error = "User not found." });
            }

            var position = await _context.Positions.FindAsync(dto.PositionId);
            if (position == null)
            {
                return BadRequest(new { Error = "Position not found." });
            }

            user.PositionId = dto.PositionId;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Position updated successfully." });
        }
    }
}