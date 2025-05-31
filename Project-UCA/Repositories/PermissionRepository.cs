using Microsoft.EntityFrameworkCore;
using Project_UCA.Data;
using Project_UCA.Middleware;
using Project_UCA.Models;
using Project_UCA.Repositories.Interfaces;


namespace Project_UCA.Repositories
{
    public class PermissionRepository : IPermissionRepository
    {
        private readonly ApplicationDbContext _context;

        public PermissionRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<List<string>> GetUserPermissionsAsync(int userId)
        {
            if (!await _context.Users.AsNoTracking().AnyAsync(u => u.Id == userId))
                throw new ArgumentException($"User with ID {userId} not found.");

            var rolePermissions = from ur in _context.UserRoles
                                  join rp in _context.RolePermissions on ur.RoleId equals rp.RoleId
                                  join p in _context.Permissions on rp.PermissionId equals p.Id
                                  where ur.UserId == userId
                                  select p.Name;

            var positionPermissions = from u in _context.Users
                                      join pp in _context.PositionPermissions on u.PositionId equals pp.PositionId
                                      join p in _context.Permissions on pp.PermissionId equals p.Id
                                      where u.Id == userId && u.PositionId != null
                                      select p.Name;

            var userPermissions = from up in _context.UserPermissions
                                  join p in _context.Permissions on up.PermissionId equals p.Id
                                  where up.UserId == userId
                                  select p.Name;

            var permissions = await rolePermissions
                .Union(positionPermissions)
                .Union(userPermissions)
                .Distinct()
                .AsNoTracking()
                .ToListAsync();

            return permissions;
        }

        public async Task<List<Permission>> GetAllPermissionsAsync()
        {
            return await _context.Permissions
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<int> GetPermissionIdByNameAsync(string permissionName)
        {
            var permission = await _context.Permissions
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Name == permissionName);

            if (permission == null)
                throw new ArgumentException($"Permission '{permissionName}' not found.");

            return permission.Id;
        }

        public async Task AddUserPermissionAsync(int userId, int permissionId)
        {
            if (!await _context.Users.AsNoTracking().AnyAsync(u => u.Id == userId))
                throw new ArgumentException($"User with ID {userId} not found.");

            if (!await _context.Permissions.AsNoTracking().AnyAsync(p => p.Id == permissionId))
                throw new ArgumentException($"Permission with ID {permissionId} not found.");

            if (await _context.UserPermissions.AnyAsync(up => up.UserId == userId && up.PermissionId == permissionId))
                throw new BadRequestException("Permission already assigned to user.");

            _context.UserPermissions.Add(new UserPermission { UserId = userId, PermissionId = permissionId });
            await _context.SaveChangesAsync();
        }

        public async Task RemoveUserPermissionAsync(int userId, int permissionId)
        {
            if (!await _context.Users.AsNoTracking().AnyAsync(u => u.Id == userId))
                throw new ArgumentException($"User with ID {userId} not found.");

            if (!await _context.Permissions.AsNoTracking().AnyAsync(p => p.Id == permissionId))
                throw new ArgumentException($"Permission with ID {permissionId} not found.");

            var userPermission = await _context.UserPermissions
                .FirstOrDefaultAsync(up => up.UserId == userId && up.PermissionId == permissionId);

            if (userPermission == null)
                throw new BadRequestException("Permission not assigned to user.");

            _context.UserPermissions.Remove(userPermission);
            await _context.SaveChangesAsync();
        }
    }
}