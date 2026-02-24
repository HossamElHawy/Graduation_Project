using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IPS_PROJECT.Data;
using IPS_PROJECT.Services;

namespace IPS_PROJECT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly PdfReportService _reportService;

        public ReportsController(AppDbContext context, PdfReportService reportService)
        {
            _context = context;
            _reportService = reportService;
        }

        [HttpGet("DownloadLogsPdf")]
        public async Task<IActionResult> DownloadLogsPdf()
        {
            try
            {
                // 1. جلب البيانات والتأكد أنها ليست فارغة
                var events = await _context.Events.OrderByDescending(x => x.Timestamp).Take(100).ToListAsync();

                if (events == null || !events.Any())
                {
                    return Content("No events found to generate report.");
                }

                int threats = events.Count(x => x.Status == "Blocked");
                int benign = events.Count(x => x.Status == "Allowed" || x.Status == "benign");

                // 2. توليد الـ PDF
                var reportService = new PdfReportService();
                byte[] pdfData = reportService.GenerateExecutiveReport(events, threats, benign);

                // 3. إرسال الملف مع تحديد النوع بدقة
                return File(pdfData, "application/pdf", "IPS_Security_Log.pdf");
            }
            catch (Exception ex)
            {
                // إذا حدث خطأ سيظهر لك في المتصفح بدلاً من تحميل ملف تالف
                return BadRequest(ex.Message);
            }
        }
    }
}