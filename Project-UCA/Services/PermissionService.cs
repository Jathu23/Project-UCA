using Project_UCA.DTOs;
using Project_UCA.Middleware;
using Project_UCA.Models;
using Project_UCA.Repositories.Interfaces;
using Project_UCA.Services.Interfaces;


namespace Project_UCA.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly IPermissionRepository _permissionRepository;

        public PermissionService(IPermissionRepository permissionRepository)
        {
            _permissionRepository = permissionRepository ?? throw new ArgumentNullException(nameof(permissionRepository));
        }

        public async Task<bool> HasPermissionAsync(int userId, string permissionName)
        {
            if (userId <= 0)
                throw new ArgumentException("Invalid user ID.");
            if (string.IsNullOrWhiteSpace(permissionName))
                throw new ArgumentException("Permission name cannot be empty.");

            var permissions = await _permissionRepository.GetUserPermissionsAsync(userId);
            return permissions.Contains(permissionName);
        }

        public async Task<List<string>> GetUserPermissionsAsync(int userId)
        {
            if (userId <= 0)
                throw new ArgumentException("Invalid user ID.");

            return await _permissionRepository.GetUserPermissionsAsync(userId);
        }

        public async Task<List<Permission>> GetAllPermissionsAsync()
        {
            return await _permissionRepository.GetAllPermissionsAsync();
        }

        public async Task AssignPermissionAsync(AssignPermissionDto dto, int callerUserId)
        {
            if (dto == null)
                throw new BadRequestException("Invalid permission data.");

            if (dto.UserId <= 0 || string.IsNullOrWhiteSpace(dto.PermissionName))
                throw new BadRequestException("Invalid user ID or permission name.");

            var permissionId = await _permissionRepository.GetPermissionIdByNameAsync(dto.PermissionName);
            await _permissionRepository.AddUserPermissionAsync(dto.UserId, permissionId);
        }

        public async Task RemovePermissionAsync(AssignPermissionDto dto, int callerUserId)
        {
            if (dto == null)
                throw new BadRequestException("Invalid permission data.");

            if (dto.UserId <= 0 || string.IsNullOrWhiteSpace(dto.PermissionName))
                throw new BadRequestException("Invalid user ID or permission name.");

            var permissionId = await _permissionRepository.GetPermissionIdByNameAsync(dto.PermissionName);
            await _permissionRepository.RemoveUserPermissionAsync(dto.UserId, permissionId);
        }
    }
}