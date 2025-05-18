using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Project_UCA.Data;
using Project_UCA.Models;

namespace Project_UCA.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("api/permissions")]
    public class PermissionsController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public PermissionsController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

       // [Authorize(Policy = "ManagePermissions")]
        [HttpPost]
        public async Task<IActionResult> CreatePermission([FromBody] CreatePermissionDto dto)
        {
            var permission = new Permission
            {
                Name = dto.Name,
                Description = dto.Description
            };
            _dbContext.Permissions.Add(permission);
            await _dbContext.SaveChangesAsync();
            return Ok();
        }
    }
}
