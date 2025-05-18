using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project_UCA.Services.Interfaces
{
    public interface IPermissionService
    {
        Task<bool> HasPermissionAsync(int userId, string permissionName);
        Task<List<string>> GetUserPermissionsAsync(int userId);
    }
}