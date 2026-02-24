using IPS_PROJECT.Data;
using IPS_PROJECT.Models;
using IPS_PROJECT.Models.ViewModels;
using IPS_PROJECT.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IPS_PROJECT.Controllers
{
    [Authorize(Roles = "Admin")] // يظهر فقط للـ Admin
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Dashboard
        public async Task<IActionResult> Index()
        {
            var totalEvents = await _context.Events.CountAsync();

            var blockedThreats = await _context.Alerts
                .Include(a => a.Threat)
                .CountAsync();

            var benignTraffic = await _context.Events
                .Where(e => e.Prediction == "Benign")
                .CountAsync();

            // Recent 10 Events
            var recentEvents = await _context.Events
                .OrderByDescending(e => e.Timestamp)
                .Take(10)
                .ToListAsync();

            // Git Last 20 Notifications From AlertNotifications Table 
            var notifications = await _context.AlertNotifications
                .OrderByDescending(n => n.Timestamp)
                .Take(20)
                .ToListAsync();

            // Build ViewModel
            var viewModel = new DashboardViewModel
            {
                TotalEvents = totalEvents,
                ThreatsBlocked = blockedThreats,
                BenignTraffic = benignTraffic,
                ModelAccuracy = 0.94, // مثال ثابت، بعدين ممكن تجيبه من ModelService
                AutoBlocking = true,  // مثال، بعدين ممكن تجيبه من Configuration DB
                DetectionThreshold = 0.8,
                ModelStatus = "Active",
                RecentEvents = recentEvents,
                // For AlertNotifications
                AlertNotifications = notifications
            };

            return View(viewModel);
        }
    }
}
