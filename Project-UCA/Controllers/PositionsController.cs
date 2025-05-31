using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project_UCA.DTO;
using Project_UCA.DTOs;
using Project_UCA.Middleware;
using Project_UCA.Services.Interfaces;
using System.Security.Authentication;
using System.Security.Claims;

namespace Project_UCA.Controllers
{
    [Route("api/positions")]
    [ApiController]
    [Authorize]
    public class PositionsController : ControllerBase
    {
        private readonly IPositionService _positionService;

        public PositionsController(IPositionService positionService)
        {
            _positionService = positionService ?? throw new ArgumentNullException(nameof(positionService));
        }

        [HttpPost]
        public async Task<IActionResult> CreatePosition([FromBody] CreatePositionDto dto)
        {
            if (!ModelState.IsValid)
                throw new ValidationException("Invalid input.", GetModelStateErrors());

            if (!TryGetCallerUserId(out int callerUserId))
                throw new AuthenticationException("Invalid user ID in token.");

            await _positionService.CreatePositionAsync(dto, callerUserId);
            return CreatedAtAction(nameof(GetPosition), new { id = 0 }, null); // ID placeholder
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPositions([FromQuery] bool includePermissions = false)
        {
            if (!TryGetCallerUserId(out int callerUserId))
                throw new AuthenticationException("Invalid user ID in token.");

            var positions = await _positionService.GetAllPositionsAsync(callerUserId, includePermissions);
            return Ok(positions);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetPosition(int id, [FromQuery] bool includePermissions = false)
        {
            if (!TryGetCallerUserId(out int callerUserId))
                throw new AuthenticationException("Invalid user ID in token.");

            var position = await _positionService.GetPositionByIdAsync(id, callerUserId, includePermissions);
            return Ok(position);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdatePosition(int id, [FromBody] UpdatePositionDto dto)
        {
            if (!ModelState.IsValid)
                throw new ValidationException("Invalid input.", GetModelStateErrors());

            if (!TryGetCallerUserId(out int callerUserId))
                throw new AuthenticationException("Invalid user ID in token.");

            await _positionService.UpdatePositionAsync(id, dto, callerUserId);
            return Ok(null);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeletePosition(int id)
        {
            if (!TryGetCallerUserId(out int callerUserId))
                throw new AuthenticationException("Invalid user ID in token.");

            await _positionService.DeletePositionAsync(id, callerUserId);
            return Ok(null);
        }

        [HttpPost("{id:int}/permissions")]
        public async Task<IActionResult> AddPositionPermissions(int id, [FromBody] List<int> permissionIds)
        {
            if (!ModelState.IsValid)
                throw new ValidationException("Invalid input.", GetModelStateErrors());

            if (!TryGetCallerUserId(out int callerUserId))
                throw new AuthenticationException("Invalid user ID in token.");

            await _positionService.AddPositionPermissionsAsync(id, permissionIds, callerUserId);
            return Ok(null);
        }

        [HttpDelete("{id:int}/permissions")]
        public async Task<IActionResult> RemovePositionPermissions(int id, [FromBody] List<int> permissionIds)
        {
            if (!ModelState.IsValid)
                throw new ValidationException("Invalid input.", GetModelStateErrors());

            if (!TryGetCallerUserId(out int callerUserId))
                throw new AuthenticationException("Invalid user ID in token.");

            await _positionService.RemovePositionPermissionsAsync(id, permissionIds, callerUserId);
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