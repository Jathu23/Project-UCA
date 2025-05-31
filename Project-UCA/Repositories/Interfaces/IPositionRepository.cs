using Project_UCA.Models;

namespace Project_UCA.Repositories.Interfaces
{
    public interface IPositionRepository
    {
        Task<bool> NameExistsAsync(string name, int? excludeId = null);
        Task<Position> CreatePositionAsync(Position position, List<int> permissionIds);
        Task<List<Position>> GetAllPositionsAsync(bool includePermissions);
        Task<Position> GetPositionByIdAsync(int id, bool includePermissions);
        Task UpdatePositionAsync(Position position);
        Task DeletePositionAsync(int id);
        Task AddPositionPermissionsAsync(int positionId, List<int> permissionIds);
        Task RemovePositionPermissionsAsync(int positionId, List<int> permissionIds);
    }
}