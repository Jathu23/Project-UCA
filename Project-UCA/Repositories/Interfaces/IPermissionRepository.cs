using Project_UCA.Models;

namespace Project_UCA.Repositories.Interfaces
{
    public interface IPermissionRepository
    {
        Task<List<string>> GetUserPermissionsAsync(int userId);
        Task<List<Permission>> GetAllPermissionsAsync();
        Task<int> GetPermissionIdByNameAsync(string permissionName);
        Task AddUserPermissionAsync(int userId, int permissionId);
        Task RemoveUserPermissionAsync(int userId, int permissionId);
    }
}