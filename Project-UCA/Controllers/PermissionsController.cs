using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project_UCA.DTOs;
using Project_UCA.Middleware;
using Project_UCA.Services.Interfaces;
using System.Security.Authentication;
using System.Security.Claims;


namespace Project_UCA.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PermissionsController : ControllerBase
    {
        private readonly IPermissionService _permissionService;

        public PermissionsController(IPermissionService permissionService)
        {
            _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPermissions()
        {
            if (!TryGetCallerUserId(out int callerUserId))
                throw new AuthenticationException("Invalid user ID in token.");

            if (!await _permissionService.HasPermissionAsync(callerUserId, "ManagePermissions"))
                throw new UnauthorizedAccessException("You do not have permission to view permissions.");

            var permissions = await _permissionService.GetAllPermissionsAsync();
            return Ok(permissions);
        }

        [HttpGet("user/{userId:int}")]
        public async Task<IActionResult> GetUserPermissions(int userId)
        {
            if (!TryGetCallerUserId(out int callerUserId))
                throw new AuthenticationException("Invalid user ID in token.");

            if (!await _permissionService.HasPermissionAsync(callerUserId, "ManagePermissions"))
                throw new UnauthorizedAccessException("You do not have permission to view permissions.");

            var permissions = await _permissionService.GetUserPermissionsAsync(userId);
            return Ok(permissions);
        }

        [HttpPost("assign")]
        public async Task<IActionResult> AssignPermission([FromBody] AssignPermissionDto dto)
        {
            if (!ModelState.IsValid)
                throw new ValidationException("Invalid input.", GetModelStateErrors());

            if (!TryGetCallerUserId(out int callerUserId))
                throw new AuthenticationException("Invalid user ID in token.");

            if (!await _permissionService.HasPermissionAsync(callerUserId, "ManagePermissions"))
                throw new UnauthorizedAccessException("You do not have permission to manage permissions.");

            await _permissionService.AssignPermissionAsync(dto, callerUserId);
            return Ok(null);
        }

        [HttpPost("remove")]
        public async Task<IActionResult> RemovePermission([FromBody] AssignPermissionDto dto)
        {
            if (!ModelState.IsValid)
                throw new ValidationException("Invalid input.", GetModelStateErrors());

            if (!TryGetCallerUserId(out int callerUserId))
                throw new AuthenticationException("Invalid user ID in token.");

            if (!await _permissionService.HasPermissionAsync(callerUserId, "ManagePermissions"))
                throw new UnauthorizedAccessException("You do not have permission to manage permissions.");

            await _permissionService.RemovePermissionAsync(dto, callerUserId);
            return Ok(null);
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