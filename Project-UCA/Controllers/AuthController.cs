using Microsoft.AspNetCore.Mvc;
using Project_UCA.DTOs;
using Project_UCA.Services.Interfaces;
using System.Threading.Tasks;

namespace Project_UCA.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (success, token, errorMessage) = await _authService.LoginAsync(loginDto);
            if (!success)
            {
                return Unauthorized(new { Error = errorMessage });
            }

            return Ok(new { Token = token });
        }
    }
}