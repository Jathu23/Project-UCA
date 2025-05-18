using Microsoft.EntityFrameworkCore;
using Project_UCA.Data;
using Project_UCA.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project_UCA.Repositories
{
    public class PermissionRepository : IPermissionRepository
    {
        private readonly ApplicationDbContext _context;

        public PermissionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<string>> GetUserPermissionsAsync(int userId)
        {
            // Get RolePermissions
            var rolePermissions = await (from ur in _context.UserRoles
                                         join rp in _context.RolePermissions on ur.RoleId equals rp.RoleId
                                         join p in _context.Permissions on rp.PermissionId equals p.Id
                                         where ur.UserId == userId
                                         select p.Name).ToListAsync();

            // Get PositionPermissions
            var positionPermissions = await (from u in _context.Users
                                             join pp in _context.PositionPermissions on u.PositionId equals pp.PositionId
                                             join p in _context.Permissions on pp.PermissionId equals p.Id
                                             where u.Id == userId && u.PositionId != null
                                             select p.Name).ToListAsync();

            // Get UserPermissions
            var userPermissions = await (from up in _context.UserPermissions
                                         join p in _context.Permissions on up.PermissionId equals p.Id
                                         where up.UserId == userId
                                         select p.Name).ToListAsync();

            // Combine and remove duplicates
            return rolePermissions
                .Union(positionPermissions)
                .Union(userPermissions)
                .Distinct()
                .ToList();
        }

        public async Task<bool> AddUserPermissionAsync(int userId, int permissionId)
        {
            if (!await _context.UserPermissions.AnyAsync(up => up.UserId == userId && up.PermissionId == permissionId))
            {
                _context.UserPermissions.Add(new Models.UserPermission { UserId = userId, PermissionId = permissionId });
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> RemoveUserPermissionAsync(int userId, int permissionId)
        {
            var userPermission = await _context.UserPermissions
                .FirstOrDefaultAsync(up => up.UserId == userId && up.PermissionId == permissionId);
            if (userPermission != null)
            {
                _context.UserPermissions.Remove(userPermission);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
    }
}