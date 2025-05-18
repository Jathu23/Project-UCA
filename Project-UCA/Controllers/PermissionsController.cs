using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project_UCA.Data;
using Project_UCA.DTOs;
using Project_UCA.Repositories.Interfaces;
using Project_UCA.Services.Interfaces;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Project_UCA.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PermissionsController : ControllerBase
    {
        private readonly IPermissionRepository _permissionRepository;
        private readonly IPermissionService _permissionService;
        private readonly ApplicationDbContext _context;

        public PermissionsController(
            IPermissionRepository permissionRepository,
            IPermissionService permissionService,
            ApplicationDbContext context)
        {
            _permissionRepository = permissionRepository;
            _permissionService = permissionService;
            _context = context;
        }

        [HttpPost("assign")]
        public async Task<IActionResult> AssignPermission([FromBody] AssignPermissionDto dto)
        {
            var callerUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            if (!await _permissionService.HasPermissionAsync(callerUserId, "ManagePermissions"))
            {
                return Forbid("You do not have permission to manage permissions.");
            }

            var permission = await _context.Permissions.FirstOrDefaultAsync(p => p.Name == dto.PermissionName);
            if (permission == null)
            {
                return BadRequest(new { Error = "Invalid permission name." });
            }

            var success = await _permissionRepository.AddUserPermissionAsync(dto.UserId, permission.Id);
            if (!success)
            {
                return BadRequest(new { Error = "Permission already assigned or invalid user." });
            }

            return Ok(new { Message = "Permission assigned successfully." });
        }

        [HttpPost("remove")]
        public async Task<IActionResult> RemovePermission([FromBody] AssignPermissionDto dto)
        {
            var callerUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            if (!await _permissionService.HasPermissionAsync(callerUserId, "ManagePermissions"))
            {
                return Forbid("You do not have permission to manage permissions.");
            }

            var permission = await _context.Permissions.FirstOrDefaultAsync(p => p.Name == dto.PermissionName);
            if (permission == null)
            {
                return BadRequest(new { Error = "Invalid permission name." });
            }

            var success = await _permissionRepository.RemoveUserPermissionAsync(dto.UserId, permission.Id);
            if (!success)
            {
                return BadRequest(new { Error = "Permission not assigned to user." });
            }

            return Ok(new { Message = "Permission removed successfully." });
        }
    }
}