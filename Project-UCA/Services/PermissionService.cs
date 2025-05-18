using Microsoft.Extensions.Logging;
using Project_UCA.Repositories.Interfaces;
using Project_UCA.Services.Interfaces;
using System.Threading.Tasks;

namespace Project_UCA.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly IPermissionRepository _permissionRepository;
        private readonly ILogger<PermissionService> _logger;

        public PermissionService(IPermissionRepository permissionRepository, ILogger<PermissionService> logger)
        {
            _permissionRepository = permissionRepository;
            _logger = logger;
        }

        public async Task<bool> HasPermissionAsync(int userId, string permissionName)
        {
            var permissions = await _permissionRepository.GetUserPermissionsAsync(userId);
            var hasPermission = permissions.Contains(permissionName);
            _logger.LogInformation("User {UserId} checked for permission {PermissionName}: {HasPermission}", userId, permissionName, hasPermission);
            return hasPermission;
        }

        public async Task<List<string>> GetUserPermissionsAsync(int userId)
        {
            var permissions = await _permissionRepository.GetUserPermissionsAsync(userId);
            _logger.LogInformation("Permissions retrieved for user {UserId}. Count: {Count}", userId, permissions.Count);
            return permissions;
        }
    }
}