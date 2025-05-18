using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project_UCA.Repositories.Interfaces
{
    public interface IPermissionRepository
    {
        Task<List<string>> GetUserPermissionsAsync(int userId);
        Task<bool> AddUserPermissionAsync(int userId, int permissionId);
        Task<bool> RemoveUserPermissionAsync(int userId, int permissionId);
    }
}