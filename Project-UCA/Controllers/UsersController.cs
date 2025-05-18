using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project_UCA.DTOs;
using Project_UCA.Services.Interfaces;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Project_UCA.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Require authentication
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] UserCreateDto userDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get caller user ID from JWT claims
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
    }
}