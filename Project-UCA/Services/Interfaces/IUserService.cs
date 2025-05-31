using Microsoft.AspNetCore.Http;
using Project_UCA.DTO;
using Project_UCA.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project_UCA.Services.Interfaces
{
    public interface IUserService
    {
        Task CreateUserAsync(UserCreateDto userDto, int callerUserId);
        Task<List<UserResponseDto>> GetUsersAsync(
            int callerUserId, string searchTerm, string role, int? positionId,
            string sortBy, bool sortDescending, int skip, int take,
            bool includeAddress, bool includeAccountDetails, bool includeInvoiceHistory, bool includeInvoiceData);
        Task<UserResponseDto> GetUserAsync(int callerUserId, int userId);
        Task AddAccountDetailsAsync(AccountDetailsDto_Request dto, int callerUserId);
        Task AddAddressAsync(AddressDto_Request dto, int callerUserId);
        Task AddInvoiceDataAsync(InvoiceDataDto_Request dto, int callerUserId);
        Task AddSignatureAsync(IFormFile file, int userId, int callerUserId);
        Task UpdateAccountDetailsAsync(AccountDetailsDto_Request dto, int callerUserId);
        Task UpdateAddressAsync(AddressDto_Request dto, int callerUserId);
        Task UpdateInvoiceDataAsync(InvoiceDataDto_Request dto, int callerUserId);
        Task UpdatePositionAsync(UpdateUserPositionDto dto, int callerUserId);
    }
}