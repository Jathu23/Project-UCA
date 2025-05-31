using Project_UCA.Controllers;
using Project_UCA.DTO;
using Project_UCA.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project_UCA.Services.Interfaces
{
    public interface IPositionService
    {
        Task CreatePositionAsync(CreatePositionDto dto, int callerUserId);
        Task<List<PositionResponseDto>> GetAllPositionsAsync(int callerUserId, bool includePermissions);
        Task<PositionResponseDto> GetPositionByIdAsync(int id, int callerUserId, bool includePermissions);
        Task UpdatePositionAsync(int id, UpdatePositionDto dto, int callerUserId);
        Task DeletePositionAsync(int id, int callerUserId);
        Task AddPositionPermissionsAsync(int positionId, List<int> permissionIds, int callerUserId);
        Task RemovePositionPermissionsAsync(int positionId, List<int> permissionIds, int callerUserId);
    }
}