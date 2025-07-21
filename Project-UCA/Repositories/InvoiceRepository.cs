
 using Microsoft.EntityFrameworkCore;
using Project_UCA.Data;
using Project_UCA.Models;
using Project_UCA.Repositories.Interfaces;

namespace Project_UCA.Repositories
{
    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly ApplicationDbContext _context;

        public InvoiceRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApplicationUser?> GetUserByIdAsync(int userId)
        {
            return await _context.Users
                .Include(u => u.Address)
                .Include(u => u.AccountDetails)
                .Include(u => u.InvoiceData)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<InvoiceHistory?> GetLatestInvoiceForUserAsync(int userId, int year, int month)
        {
            return await _context.InvoiceHistories
                .Include(i => i.InvoiceData)
                .Where(i => i.UserId == userId && i.Timestamp.Year == year && i.Timestamp.Month == month)
                .OrderByDescending(i => i.Id)
                .FirstOrDefaultAsync();
        }

        public async Task<int> GetLastInvoiceNumberAsync(int userId)
        {
            var lastInvoice = await _context.InvoiceHistories
                .Include(i => i.InvoiceData)
                .Where(i => i.UserId == userId)
                .OrderByDescending(i => i.Id)
                .FirstOrDefaultAsync();
            if (lastInvoice == null || lastInvoice.InvoiceData == null)
                return 0;

            return int.TryParse(lastInvoice.InvoiceData.InvoiceNumber, out int number) ? number : 0;
        }

        public async Task AddInvoiceHistoryAsync(InvoiceHistory invoiceHistory)
        {
            await _context.InvoiceHistories.AddAsync(invoiceHistory);
            await _context.SaveChangesAsync();
        }

        public async Task<List<InvoiceHistory>> GetInvoiceHistoryAsync(int userId, int page, int pageSize, DateTime? startDate, DateTime? endDate)
        {
            var query = _context.InvoiceHistories
                .Include(i => i.InvoiceData)
                .Where(i => i.UserId == userId);

            if (startDate.HasValue)
                query = query.Where(i => i.Timestamp >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(i => i.Timestamp <= endDate.Value);

            return await query
                .OrderByDescending(i => i.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}
    