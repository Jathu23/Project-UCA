using System.Threading.Tasks;

namespace Project_UCA.Services.Interfaces
{
    public interface IPermissionService
    {
        Task<bool> HasPermissionAsync(int userId, string permissionName);
    }
}