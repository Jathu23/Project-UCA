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
            var callerUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(callerUserIdClaim, out int callerUserId))
            {
                return Unauthorized("Invalid user ID in token.");
            }

            var (success, users, errorMessage) = await _userService.GetUsersAsync(
                callerUserId,
                searchTerm,
                role,
                positionId,
                sortBy,
                sortDescending,
                skip,
                take,
                includeAddress,
                includeAccountDetails,
                includeInvoiceHistory,
                includeInvoiceData);

            if (!success)
            {
                return BadRequest(new { Error = errorMessage });
            }

            return Ok(users);
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUser(int userId)
        {
            var callerUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(callerUserIdClaim, out int callerUserId))
            {
                return Unauthorized("Invalid user ID in token.");
            }

            var (success, user, errorMessage) = await _userService.GetUserAsync(callerUserId, userId);
            if (!success)
            {
                return BadRequest(new { Error = errorMessage });
            }

            return Ok(user);
        }

        [HttpPost("add_AccountDetails")]
        public async Task<IActionResult> AddAccountDetails([FromBody] AccountDetailsDto_Request accountDetailsDto)
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

            var (success, errorMessage) = await _userService.AddAccountDetailsAsync(accountDetailsDto, callerUserId);
            if (!success)
            {
                return BadRequest(new { Error = errorMessage });
            }

            return Ok(new { Message = errorMessage });
        }

        [HttpPost("add_Address")]
        public async Task<IActionResult> AddAddress([FromBody] AddressDto_Request addressDto)
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

            var (success, errorMessage) = await _userService.AddAddressAsync(addressDto, callerUserId);
            if (!success)
            {
                return BadRequest(new { Error = errorMessage });
            }

            return Ok(new { Message = errorMessage });
        }

        [HttpPost("add_Invoicedata")]
        public async Task<IActionResult> AddInvoiceData([FromBody] InvoiceDataDto_Request invoiceDataDto)
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

            var (success, errorMessage) = await _userService.AddInvoiceDataAsync(invoiceDataDto, callerUserId);
            if (!success)
            {
                return BadRequest(new { Error = errorMessage });
            }

            return Ok(new { Message = errorMessage });
        }

        [HttpPost("add_signature")]
        public async Task<IActionResult> AddSignature( IFormFile file,  int userId)
        {
            var callerUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(callerUserIdClaim, out int callerUserId))
            {
                return Unauthorized("Invalid user ID in token.");
            }

            var (success, errorMessage) = await _userService.AddSignatureAsync(file, userId, callerUserId);
            if (!success)
            {
                return BadRequest(new { Error = errorMessage });
            }

            return Ok(new { Message = errorMessage });
        }

        [HttpPatch("update_AccountDetails")]
        public async Task<IActionResult> UpdateAccountDetails([FromBody] AccountDetailsDto_Request accountDetailsDto)
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

            var (success, errorMessage) = await _userService.UpdateAccountDetailsAsync(accountDetailsDto, callerUserId);
            if (!success)
            {
                return BadRequest(new { Error = errorMessage });
            }

            return Ok(new { Message = errorMessage });
        }

        [HttpPatch("update_Address")]
        public async Task<IActionResult> UpdateAddress([FromBody] AddressDto_Request addressDto)
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

            var (success, errorMessage) = await _userService.UpdateAddressAsync(addressDto, callerUserId);
            if (!success)
            {
                return BadRequest(new { Error = errorMessage });
            }

            return Ok(new { Message = errorMessage });
        }

        [HttpPatch("update_InvoiceData")]
        public async Task<IActionResult> UpdateInvoiceData([FromBody] InvoiceDataDto_Request invoiceDataDto)
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

            var (success, errorMessage) = await _userService.UpdateInvoiceDataAsync(invoiceDataDto, callerUserId);
            if (!success)
            {
                return BadRequest(new { Error = errorMessage });
            }

            return Ok(new { Message = errorMessage });
        }
    }
}