using Project_UCA.Controllers;
using Project_UCA.DTO;
using Project_UCA.DTOs;
using Project_UCA.Middleware;
using Project_UCA.Models;
using Project_UCA.Repositories.Interfaces;
using Project_UCA.Services.Interfaces;

namespace Project_UCA.Services
{
    public class PositionService : IPositionService
    {
        private readonly IPositionRepository _positionRepository;
        private readonly IPermissionService _permissionService;

        public PositionService(IPositionRepository positionRepository, IPermissionService permissionService)
        {
            _positionRepository = positionRepository ?? throw new ArgumentNullException(nameof(positionRepository));
            _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
        }

        public async Task CreatePositionAsync(CreatePositionDto dto, int callerUserId)
        {
            if (dto == null)
                throw new BadRequestException("Invalid position data.");

            if (!await _permissionService.HasPermissionAsync(callerUserId, "ManagePositions"))
                throw new UnauthorizedAccessException("You do not have permission to manage positions.");

            var position = new Position
            {
                Name = dto.Name,
                Description = dto.Description
                
            };

            await _positionRepository.CreatePositionAsync(position, dto.PermissionIds ?? new List<int>());
        }

        public async Task<List<PositionResponseDto>> GetAllPositionsAsync(int callerUserId, bool includePermissions)
        {
            if (!await _permissionService.HasPermissionAsync(callerUserId, "ManagePositions"))
                throw new UnauthorizedAccessException("You do not have permission to view positions.");

            var positions = await _positionRepository.GetAllPositionsAsync(includePermissions);
            return positions.Select(p => MapToResponseDto(p)).ToList();
        }

        public async Task<PositionResponseDto> GetPositionByIdAsync(int id, int callerUserId, bool includePermissions)
        {
            if (!await _permissionService.HasPermissionAsync(callerUserId, "ManagePositions"))
                throw new UnauthorizedAccessException("You do not have permission to view positions.");

            var position = await _positionRepository.GetPositionByIdAsync(id, includePermissions);
            return MapToResponseDto(position);
        }

        public async Task UpdatePositionAsync(int id, UpdatePositionDto dto, int callerUserId)
        {
            if (dto == null)
                throw new BadRequestException("Invalid position data.");

            if (!await _permissionService.HasPermissionAsync(callerUserId, "ManagePositions"))
                throw new UnauthorizedAccessException("You do not have permission to manage positions.");

            var position = new Position
            {
                Id = id,
                Name = dto.Name,
                Description = dto.Description
            };

            await _positionRepository.UpdatePositionAsync(position);
        }

        public async Task DeletePositionAsync(int id, int callerUserId)
        {
            if (!await _permissionService.HasPermissionAsync(callerUserId, "ManagePositions"))
                throw new UnauthorizedAccessException("You do not have permission to manage positions.");

            await _positionRepository.DeletePositionAsync(id);
        }

        public async Task AddPositionPermissionsAsync(int positionId, List<int> permissionIds, int callerUserId)
        {
            if (permissionIds == null || !permissionIds.Any())
                throw new BadRequestException("No permission IDs provided.");

            if (!await _permissionService.HasPermissionAsync(callerUserId, "ManagePositions"))
                throw new UnauthorizedAccessException("You do not have permission to manage positions.");

            await _positionRepository.AddPositionPermissionsAsync(positionId, permissionIds);
        }

        public async Task RemovePositionPermissionsAsync(int positionId, List<int> permissionIds, int callerUserId)
        {
            if (permissionIds == null || !permissionIds.Any())
                throw new BadRequestException("No permission IDs provided.");

            if (!await _permissionService.HasPermissionAsync(callerUserId, "ManagePositions"))
                throw new UnauthorizedAccessException("You do not have permission to manage positions.");

            await _positionRepository.RemovePositionPermissionsAsync(positionId, permissionIds);
        }

        private PositionResponseDto MapToResponseDto(Position position)
        {
            return new PositionResponseDto
            {
                Id = position.Id,
                Name = position.Name,
                Description = position.Description,
                PermissionIds = position.PositionPermissions?.Select(pp => pp.PermissionId).ToList() ?? new List<int>(),
                Permissions = position.PositionPermissions?.Select(pp => new PermissionDto
                {
                    Id = pp.Permission.Id,
                    Name = pp.Permission.Name,
                    Description = pp.Permission.Description
                }).ToList() ?? new List<PermissionDto>()
            };
        }
    }
}