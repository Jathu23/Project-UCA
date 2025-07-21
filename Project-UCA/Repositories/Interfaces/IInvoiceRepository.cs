using Project_UCA.Models;
using System.Collections.Generic;

namespace Project_UCA.Repositories.Interfaces
{
    public interface IInvoiceRepository
    {
        Task<ApplicationUser?> GetUserByIdAsync(int userId);
        Task<InvoiceHistory?> GetLatestInvoiceForUserAsync(int userId, int year, int month);
        //Task GetLastInvoiceNumberAsync(int userId);
        Task AddInvoiceHistoryAsync(InvoiceHistory invoiceHistory);
        Task<List<InvoiceHistory?>> GetInvoiceHistoryAsync(int userId, int page, int pageSize, DateTime? startDate, DateTime? endDate);
    }
}
