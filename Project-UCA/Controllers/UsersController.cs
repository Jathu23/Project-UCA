using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project_UCA.DTO;
using Project_UCA.DTOs;
using Project_UCA.Middleware;
using Project_UCA.Services.Interfaces;
using System.Security.Authentication;
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

        public UsersController(IUserService userService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] UserCreateDto userDto)
        {
            if (!ModelState.IsValid)
                throw new ValidationException("Invalid input.", GetModelStateErrors());

            if (!TryGetCallerUserId(out int callerUserId))
                throw new AuthenticationException("Invalid user ID in token.");

            await _userService.CreateUserAsync(userDto, callerUserId);
            return CreatedAtAction(nameof(CreateUser), new { email = userDto.Email }, null);
        }

        [HttpPost("update-position")]
        public async Task<IActionResult> UpdatePosition([FromBody] UpdateUserPositionDto dto)
        {
            if (!ModelState.IsValid)
                throw new ValidationException("Invalid input.", GetModelStateErrors());

            if (!TryGetCallerUserId(out int callerUserId))
                throw new AuthenticationException("Invalid user ID in token.");

            await _userService.UpdatePositionAsync(dto, callerUserId);
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers(
            [FromQuery] string searchTerm = "",
            [FromQuery] string role = "",
            [FromQuery] int? positionId = null,
            [FromQuery] string sortBy = "id",
            [FromQuery] bool sortDescending = false,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 10,
            [FromQuery] bool includeAddress = false,
            [FromQuery] bool includeAccountDetails = false,
            [FromQuery] bool includeInvoiceHistory = false,
            [FromQuery] bool includeInvoiceData = false)
        {
            if (skip < 0 || take <= 0)
                throw new BadRequestException("Invalid pagination parameters.");

            if (!TryGetCallerUserId(out int callerUserId))
                throw new AuthenticationException("Invalid user ID in token.");

            var users = await _userService.GetUsersAsync(
                callerUserId, searchTerm, role, positionId, sortBy, sortDescending,
                skip, take, includeAddress, includeAccountDetails,
                includeInvoiceHistory, includeInvoiceData);

            return Ok(users);
        }

        [HttpGet("{userId:int}")]
        public async Task<IActionResult> GetUser(int userId)
        {
            if (!TryGetCallerUserId(out int callerUserId))
                throw new AuthenticationException("Invalid user ID in token.");

            var user = await _userService.GetUserAsync(callerUserId, userId);
            return Ok(user);
        }

        [HttpPost("add-account-details")]
        public async Task<IActionResult> AddAccountDetails([FromBody] AccountDetailsDto_Request dto)
        {
            if (!ModelState.IsValid)
                throw new ValidationException("Invalid input.", GetModelStateErrors());

            if (!TryGetCallerUserId(out int callerUserId))
                throw new AuthenticationException("Invalid user ID in token.");

            await _userService.AddAccountDetailsAsync(dto, callerUserId);
            return Ok();
        }

        [HttpPost("add-address")]
        public async Task<IActionResult> AddAddress([FromBody] AddressDto_Request dto)
        {
            if (!ModelState.IsValid)
                throw new ValidationException("Invalid input.", GetModelStateErrors());

            if (!TryGetCallerUserId(out int callerUserId))
                throw new AuthenticationException("Invalid user ID in token.");

            await _userService.AddAddressAsync(dto, callerUserId);
            return Ok();
        }

        [HttpPost("add-invoice-data")]
        public async Task<IActionResult> AddInvoiceData([FromBody] InvoiceDataDto_Request dto)
        {
            if (!ModelState.IsValid)
                throw new ValidationException("Invalid input.", GetModelStateErrors());

            if (!TryGetCallerUserId(out int callerUserId))
                throw new AuthenticationException("Invalid user ID in token.");

            await _userService.AddInvoiceDataAsync(dto, callerUserId);
            return Ok();
        }

        [HttpPost("add-signature")]
        public async Task<IActionResult> AddSignature(IFormFile file, [FromQuery] int userId)
        {
            if (file == null || file.Length == 0)
                throw new BadRequestException("Invalid file.");

            if (!TryGetCallerUserId(out int callerUserId))
                throw new AuthenticationException("Invalid user ID in token.");

            await _userService.AddSignatureAsync(file, userId, callerUserId);
            return Ok();
        }

        [HttpPatch("update-account-details")]
        public async Task<IActionResult> UpdateAccountDetails([FromBody] AccountDetailsDto_Request dto)
        {
            if (!ModelState.IsValid)
                throw new ValidationException("Invalid input.", GetModelStateErrors());

            if (!TryGetCallerUserId(out int callerUserId))
                throw new AuthenticationException("Invalid user ID in token.");

            await _userService.UpdateAccountDetailsAsync(dto, callerUserId);
            return Ok();
        }

        [HttpPatch("update-address")]
        public async Task<IActionResult> UpdateAddress([FromBody] AddressDto_Request dto)
        {
            if (!ModelState.IsValid)
                throw new ValidationException("Invalid input.", GetModelStateErrors());

            if (!TryGetCallerUserId(out int callerUserId))
                throw new AuthenticationException("Invalid user ID in token.");

            await _userService.UpdateAddressAsync(dto, callerUserId);
            return Ok();
        }

        [HttpPatch("update-invoice-data")]
        public async Task<IActionResult> UpdateInvoiceData([FromBody] InvoiceDataDto_Request dto)
        {
            if (!ModelState.IsValid)
                throw new ValidationException("Invalid input.", GetModelStateErrors());

            if (!TryGetCallerUserId(out int callerUserId))
                throw new AuthenticationException("Invalid user ID in token.");

            await _userService.UpdateInvoiceDataAsync(dto, callerUserId);
            return Ok();
        }

        private bool TryGetCallerUserId(out int callerUserId)
        {
            var callerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(callerIdClaim, out callerUserId);
        }

        private IDictionary<string, string[]> GetModelStateErrors()
        {
            return ModelState.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
            );
        }
    }
}