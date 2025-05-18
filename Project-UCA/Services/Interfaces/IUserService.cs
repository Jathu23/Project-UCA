using Project_UCA.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project_UCA.Services.Interfaces
{
    public interface IUserService
    {
        Task<(bool Success, string ErrorMessage)> CreateUserAsync(UserCreateDto userDto, int callerUserId);
        Task<(bool Success, List<UserResponseDto> Users, string ErrorMessage)> GetUsersAsync(
            int callerUserId,
            string searchTerm,
            string role,
            int? positionId,
            string sortBy,
            bool sortDescending,
            int skip,
            int take,
            bool includeAddress,
            bool includeAccountDetails,
            bool includeInvoiceHistory,
            bool includeInvoiceData);
        Task<(bool Success, UserResponseDto User, string ErrorMessage)> GetUserAsync(int callerUserId, int userId);
        Task<(bool Success, string ErrorMessage)> AddAccountDetailsAsync(AccountDetailsDto_Request dto, int callerUserId);
        Task<(bool Success, string ErrorMessage)> AddAddressAsync(AddressDto_Request dto, int callerUserId);
        Task<(bool Success, string ErrorMessage)> AddInvoiceDataAsync(InvoiceDataDto_Request dto, int callerUserId);
        Task<(bool Success, string ErrorMessage)> AddSignatureAsync(IFormFile file, int userId, int callerUserId);
        Task<(bool Success, string ErrorMessage)> UpdateAccountDetailsAsync(AccountDetailsDto_Request dto, int callerUserId);
        Task<(bool Success, string ErrorMessage)> UpdateAddressAsync(AddressDto_Request dto, int callerUserId);
        Task<(bool Success, string ErrorMessage)> UpdateInvoiceDataAsync(InvoiceDataDto_Request dto, int callerUserId);
    }
}