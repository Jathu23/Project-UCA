using Project_UCA.DTO;

namespace Project_UCA.Services.Interface
{
    public interface IInvoiceService
    {
        Task<dynamic> GetInvoiceAsync(int userId, bool regenerate = false);
        Task<List<InvoiceHistoryDto>> GetInvoiceHistoryAsync(int userId, int page = 1, int pageSize = 10, DateTime? startDate = null, DateTime? endDate = null);
    }
}