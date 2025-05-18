using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Project_UCA.DTO;
using Project_UCA.Models;
using Project_UCA.Services;

namespace Project_UCA.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AuthService _authService;

        public AuthController(UserManager<ApplicationUser> userManager, AuthService authService)
        {
            _userManager = userManager;
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
            {
                return Unauthorized("Invalid credentials");
            }

            if (await _userManager.IsLockedOutAsync(user))
            {
                return Unauthorized("Account locked. Try again later.");
            }

            var token = await _authService.GenerateJwtToken(user);
            return Ok(new { Token = token });
        }
    }
}
