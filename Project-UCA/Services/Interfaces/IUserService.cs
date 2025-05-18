using Project_UCA.DTOs;
using System.Threading.Tasks;

namespace Project_UCA.Services.Interfaces
{
    public interface IUserService
    {
        Task<(bool Success, string ErrorMessage)> CreateUserAsync(UserCreateDto userDto);
    }
}