using Microsoft.AspNetCore.Mvc;
using Project_UCA.Services.Interface;
using Project_UCA.Utilities.Interface;
using System;
using System.Threading.Tasks;

namespace Project_UCA.Controllers
{
    [Route("api/invoices")]
    [ApiController]
    public class InvoicesController : ControllerBase
    {
        private readonly IInvoiceService _invoiceService;

        public InvoicesController(IInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetInvoice(int userId, [FromQuery] bool regenerate = false)
        {
            var invoice = await _invoiceService.GetInvoiceAsync(userId, regenerate);
            return Ok(invoice);
        }

        [HttpGet("{userId}/history")]
        public async Task<IActionResult> GetInvoiceHistory(int userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? startDate = null, [FromQuery] string? endDate = null)
        {
            DateTime? start = startDate != null ? DateTime.Parse(startDate) : null;
            DateTime? end = endDate != null ? DateTime.Parse(endDate) : null;

            var history = await _invoiceService.GetInvoiceHistoryAsync(userId, page, pageSize, start, end);
            return Ok(history);
        }
    }
}