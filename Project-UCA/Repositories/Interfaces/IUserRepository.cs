using Project_UCA.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project_UCA.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<bool> EmployeeIdExistsAsync(string employeeId);
        Task<bool> EmailExistsAsync(string email);
        Task<int> CountMasterUsersAsync();
        Task<int> CountUsersAsync(string searchTerm, string role, int? positionId);
        Task<List<ApplicationUser>> SearchUsersAsync(
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
        Task<ApplicationUser> GetUserByIdAsync(int userId, bool includeAllDetails);
        Task AddAccountDetailsAsync(AccountDetails accountDetails);
        Task AddAddressAsync(Address address);
        Task AddInvoiceDataAsync(InvoiceData invoiceData);
        Task AddSignatureAsync(int userId, string signaturePath);
        Task UpdateAccountDetailsAsync(AccountDetails accountDetails);
        Task UpdateAddressAsync(Address address);
        Task UpdateInvoiceDataAsync(InvoiceData invoiceData);
    }
}