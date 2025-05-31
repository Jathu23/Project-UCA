using Microsoft.EntityFrameworkCore;
using Project_UCA.Data;
using Project_UCA.Middleware;
using Project_UCA.Models;
using Project_UCA.Repositories.Interfaces;


namespace Project_UCA.Repositories
{
    public class PositionRepository : IPositionRepository
    {
        private readonly ApplicationDbContext _context;

        public PositionRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<bool> NameExistsAsync(string name, int? excludeId = null)
        {
            var query = _context.Positions.AsNoTracking().Where(p => p.Name.ToLower() == name.ToLower());
            if (excludeId.HasValue)
                query = query.Where(p => p.Id != excludeId.Value);
            return await query.AnyAsync();
        }

        public async Task<Position> CreatePositionAsync(Position position, List<int> permissionIds)
        {
            if (await NameExistsAsync(position.Name))
                throw new BadRequestException($"Position with name '{position.Name}' already exists.");

            _context.Positions.Add(position);
            await _context.SaveChangesAsync();

            if (permissionIds != null && permissionIds.Any())
            {
                foreach (var permissionId in permissionIds)
                {
                    if (!await _context.Permissions.AsNoTracking().AnyAsync(p => p.Id == permissionId))
                        throw new ArgumentException($"Permission with ID {permissionId} not found.");

                    _context.PositionPermissions.Add(new PositionPermission
                    {
                        PositionId = position.Id,
                        PermissionId = permissionId
                    });
                }
                await _context.SaveChangesAsync();
            }

            return position;
        }

        public async Task<List<Position>> GetAllPositionsAsync(bool includePermissions)
        {
            var query = _context.Positions.AsNoTracking();
            if (includePermissions)
                query = query.Include(p => p.PositionPermissions).ThenInclude(pp => pp.Permission);
            return await query.ToListAsync();
        }

        public async Task<Position> GetPositionByIdAsync(int id, bool includePermissions)
        {
            var query = _context.Positions.AsNoTracking();
            if (includePermissions)
                query = query.Include(p => p.PositionPermissions).ThenInclude(pp => pp.Permission);
            var position = await query.FirstOrDefaultAsync(p => p.Id == id);
            if (position == null)
                throw new ArgumentException($"Position with ID {id} not found.");
            return position;
        }

        public async Task UpdatePositionAsync(Position position)
        {
            if (await NameExistsAsync(position.Name, position.Id))
                throw new BadRequestException($"Position with name '{position.Name}' already exists.");

            var existing = await _context.Positions.FindAsync(position.Id);
            if (existing == null)
                throw new ArgumentException($"Position with ID {position.Id} not found.");

            existing.Name = position.Name;
            existing.Description = position.Description;
            await _context.SaveChangesAsync();
        }

        public async Task DeletePositionAsync(int id)
        {
            var position = await _context.Positions
                .Include(p => p.PositionPermissions)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (position == null)
                throw new ArgumentException($"Position with ID {id} not found.");

            if (await _context.Users.AnyAsync(u => u.PositionId == id))
                throw new BadRequestException("Cannot delete position assigned to users.");

            _context.PositionPermissions.RemoveRange(position.PositionPermissions);
            _context.Positions.Remove(position);
            await _context.SaveChangesAsync();
        }

        public async Task AddPositionPermissionsAsync(int positionId, List<int> permissionIds)
        {
            if (!await _context.Positions.AsNoTracking().AnyAsync(p => p.Id == positionId))
                throw new ArgumentException($"Position with ID {positionId} not found.");

            foreach (var permissionId in permissionIds)
            {
                if (!await _context.Permissions.AsNoTracking().AnyAsync(p => p.Id == permissionId))
                    throw new ArgumentException($"Permission with ID {permissionId} not found.");

                if (!await _context.PositionPermissions.AnyAsync(pp => pp.PositionId == positionId && pp.PermissionId == permissionId))
                {
                    _context.PositionPermissions.Add(new PositionPermission
                    {
                        PositionId = positionId,
                        PermissionId = permissionId
                    });
                }
            }
            await _context.SaveChangesAsync();
        }

        public async Task RemovePositionPermissionsAsync(int positionId, List<int> permissionIds)
        {
            if (!await _context.Positions.AsNoTracking().AnyAsync(p => p.Id == positionId))
                throw new ArgumentException($"Position with ID {positionId} not found.");

            foreach (var permissionId in permissionIds)
            {
                var positionPermission = await _context.PositionPermissions
                    .FirstOrDefaultAsync(pp => pp.PositionId == positionId && pp.PermissionId == permissionId);
                if (positionPermission != null)
                {
                    _context.PositionPermissions.Remove(positionPermission);
                }
            }
            await _context.SaveChangesAsync();
        }
    }
}