using Project_UCA.DTOs;
using System.Threading.Tasks;

namespace Project_UCA.Services.Interfaces
{
    public interface IAuthService
    {
        Task<(bool Success, string Token, string ErrorMessage)> LoginAsync(LoginDto loginDto);
    }
}