
using Project_UCA.DTO;
using Project_UCA.Middleware;
using Project_UCA.Models;
using Project_UCA.Repositories.Interfaces;
using Project_UCA.Services.Interface;



namespace Project_UCA.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IInvoiceRepository _invoiceRepository;
     

        public InvoiceService(IInvoiceRepository invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
            
        }

        public async Task<dynamic> GetInvoiceAsync(int userId, bool regenerate = false)
        {
            var user = await _invoiceRepository.GetUserByIdAsync(userId);
            if (user == null || user.InvoiceData == null || user.AccountDetails == null || user.Address == null)
                throw new BadRequestException("User or necessary data not found.");

            var currentDate = DateTime.UtcNow;
            var currentMonth = currentDate.Month;
            var currentYear = currentDate.Year;

            var existingInvoice = await _invoiceRepository.GetLatestInvoiceForUserAsync(userId, currentYear, currentMonth);
            string invoiceDate;
            int invoiceNumber;
            string message;

            if (existingInvoice == null || regenerate)
            {
                int lastInvoiceNumber = 1;
                invoiceNumber = lastInvoiceNumber == 0 ? 1000 : lastInvoiceNumber + 1;
                invoiceDate = currentDate.ToString("dd/MM/yyyy");
                message = "Invoice generated successfully";

                var newInvoiceHistory = new InvoiceHistory
                {
                    UserId = userId,
                    InvoiceDataId = user.InvoiceData.Id,
                    Action = "Generated",
                    Timestamp = currentDate,
                    Details = $"Invoice {invoiceNumber} created"
                };

                user.InvoiceData.InvoiceNumber = invoiceNumber.ToString();
                await _invoiceRepository.AddInvoiceHistoryAsync(newInvoiceHistory);

                // // PDF Generation (Commented: No PDF service)
                // var pdfBytes = _pdfService.GenerateInvoicePdf(new InvoiceResponseDto
                // {
                //     InvoiceNumber = invoiceNumber.ToString(),
                //     InvoiceDate = invoiceDate,
                //     NameWithInitials = $"{user.FirstName[0]}. {user.LastName}",
                //     Address = user.Address,
                //     AccountDetails = user.AccountDetails,
                //     InvoiceData = user.InvoiceData
                // });
                //
                // var publicId = $"invoices/user_{userId}_{invoiceNumber}";
                // var (tempUrl, originalUrl, _) = await _cloudinaryService.UploadImageAsync(
                //     new FormFile(new MemoryStream(pdfBytes), 0, pdfBytes.Length, "invoice", $"invoice_{invoiceNumber}.pdf"),
                //     publicId
                // );
            }
            else
            {
                invoiceNumber = int.Parse(existingInvoice.InvoiceData.InvoiceNumber);
                invoiceDate = existingInvoice.Timestamp.ToString("dd/MM/yyyy");
                message = "Invoice already exists for this month";
            }

            return new InvoiceResponseDto
            {
                InvoiceNumber = invoiceNumber.ToString(),
                InvoiceDate = invoiceDate,
                NameWithInitials = $"{user.FirstName[0]}. {user.LastName}",
                Address = user.Address,
                AccountDetails = user.AccountDetails,
                InvoiceData = user.InvoiceData
            };
        }

        public async Task<List<InvoiceHistoryDto>> GetInvoiceHistoryAsync(int userId, int page = 1, int pageSize = 10, DateTime? startDate = null, DateTime? endDate = null)
        {
            if (page < 1 || pageSize < 1)
                throw new BadRequestException("Invalid page or page size.");

            var invoices = await _invoiceRepository.GetInvoiceHistoryAsync(userId, page, pageSize, startDate, endDate);
            return invoices.Select(i => new InvoiceHistoryDto
            {
                InvoiceNumber = i.InvoiceData?.InvoiceNumber ?? string.Empty,
                InvoiceDate = i.Timestamp.ToString("dd/MM/yyyy"),
                Action = i.Action,
                Timestamp = i.Timestamp.ToString("dd/MM/yyyy HH:mm:ss")
            }).ToList();
        }

        //eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJtYXN0ZXJAZXhhbXBsZS5jb20iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJNYXN0ZXIiLCJleHAiOjE3NDg5NTI0NTQsImlzcyI6InlvdXItaXNzdWVyIiwiYXVkIjoieW91ci1hdWRpZW5jZSJ9.W2XwLJryNxIspazTZrJ7g_ixaUOynlc9hypmIgzvElU
    }
}