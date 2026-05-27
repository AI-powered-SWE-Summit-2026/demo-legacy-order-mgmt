using LegacyOrderMgmt.Core.Data;
using LegacyOrderMgmt.Core.Models;
using LegacyOrderMgmt.Core.Services;
using LegacyOrderMgmt.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace LegacyOrderMgmt.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly OrderDbContext _db;

        // Legacy: IHostingEnvironment injected — deprecated in 3.0+
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _env;

        public HomeController(OrderDbContext db, Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
        {
            _db = db;
            _env = env;
        }

        // Legacy: synchronous action — no async/await
        public IActionResult Index()
        {
            // Legacy: DateTime.Now for "today's" dashboard stats — timezone-unaware
            var today = DateTime.Now.Date;

            // Legacy: multiple separate synchronous DB calls instead of one query or projection
            var totalOrders = _db.Orders.Count();
            var pendingOrders = _db.Orders.Count(o => o.Status == 1 || o.Status == 2);
            var shippedToday = _db.Orders.Count(o => o.ShippedDate.HasValue && o.ShippedDate.Value.Date == today);
            var overdueInvoices = _db.Invoices.Count(i => i.PaymentStatus == "Pending" && i.DueDate < DateTime.Now);

            // Legacy: passing raw counts via ViewBag instead of a typed ViewModel
            ViewBag.TotalOrders = totalOrders;
            ViewBag.PendingOrders = pendingOrders;
            ViewBag.ShippedToday = shippedToday;
            ViewBag.OverdueInvoices = overdueInvoices;

            var recentOrders = _db.Orders
                .Include(o => o.Customer)
                .OrderByDescending(o => o.OrderDate)
                .Take(10)
                .ToList();   // Legacy: no AsNoTracking()

            return View(recentOrders);
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
