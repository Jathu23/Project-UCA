using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Project_UCA.Data;
using Project_UCA.Models;

namespace Project_UCA.Controllers
{
   // [Authorize]
    [ApiController]
    [Route("api/positions")]
    public class PositionsController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public PositionsController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

      //  [Authorize(Policy = "ManagePositions")]
        [HttpPost]
        public async Task<IActionResult> CreatePosition([FromBody] CreatePositionDto dto)
        {
            var position = new Position
            {
                Name = dto.Name,
                Description = dto.Description
            };
            _dbContext.Positions.Add(position);
            await _dbContext.SaveChangesAsync();

            foreach (var permissionId in dto.PermissionIds)
            {
                _dbContext.PositionPermissions.Add(new PositionPermission
                {
                    PositionId = position.Id,
                    PermissionId = permissionId
                });
            }
            await _dbContext.SaveChangesAsync();
            return Ok();
        }

      //  [Authorize(Policy = "ManagePositions")]
        [HttpPost("{id}/permissions")]
        public async Task<IActionResult> AddPositionPermissions(int id, [FromBody] List<int> permissionIds)
        {
            foreach (var permissionId in permissionIds)
            {
                _dbContext.PositionPermissions.Add(new PositionPermission
                {
                    PositionId = id,
                    PermissionId = permissionId
                });
            }
            await _dbContext.SaveChangesAsync();
            return Ok();
        }
    }


    public class CreatePermissionDto
    {
        public string Name { get; set; }
        public string? Description { get; set; }
    }

    public class CreatePositionDto
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public List<int> PermissionIds { get; set; }
    }
}
