using Microsoft.AspNetCore.Mvc;
using Project_UCA.DTOs;
using Project_UCA.Services.Interfaces;
using System.Threading.Tasks;

namespace Project_UCA.Controllers
{
    [Route("api/user")]
    [ApiController]
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

            var (success, errorMessage) = await _userService.CreateUserAsync(userDto);
            if (!success)
            {
                return BadRequest(new { Error = errorMessage });
            }

            return CreatedAtAction(nameof(CreateUser), new { email = userDto.Email }, new { Message = "User created successfully." });
        }
    }
}