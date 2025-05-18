using Project_UCA.Models;
using System.Threading.Tasks;

namespace Project_UCA.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<bool> EmployeeIdExistsAsync(string employeeId);
        Task<bool> EmailExistsAsync(string email);
        Task<ApplicationUser> GetUserByEmailAsync(string email);
        Task<int> CountMasterUsersAsync();
    }
}