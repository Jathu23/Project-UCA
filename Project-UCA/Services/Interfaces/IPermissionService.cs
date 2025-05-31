using Project_UCA.DTOs;
using Project_UCA.Models;

namespace Project_UCA.Services.Interfaces
{
    public interface IPermissionService
    {
        Task<bool> HasPermissionAsync(int userId, string permissionName);
        Task<List<string>> GetUserPermissionsAsync(int userId);
        Task<List<Permission>> GetAllPermissionsAsync();
        Task AssignPermissionAsync(AssignPermissionDto dto, int callerUserId);
        Task RemovePermissionAsync(AssignPermissionDto dto, int callerUserId);
    }
}