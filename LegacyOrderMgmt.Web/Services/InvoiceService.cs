using LegacyOrderMgmt.Core.Data;
using LegacyOrderMgmt.Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace LegacyOrderMgmt.Web.Services
{
    public class InvoiceService
    {
        private readonly OrderDbContext _db;

        public InvoiceService(OrderDbContext db)
        {
            _db = db;
        }

        // Legacy: synchronous invoice generation — no async, no background job
        public Invoice GenerateInvoice(Order order)
        {
            // Legacy: sequential DB call to get last invoice number — race condition under load
            var lastInvoice = _db.Invoices.OrderByDescending(i => i.Id).FirstOrDefault();
            var nextInvoiceNum = (lastInvoice?.Id ?? 0) + 1;

            var invoice = new Invoice
            {
                OrderId = order.Id,
                InvoiceNumber = $"INV-{DateTime.Now.Year}-{nextInvoiceNum:D6}",
                InvoiceDate = DateTime.Now,             // Legacy: DateTime.Now — timezone unaware
                DueDate = DateTime.Now.AddDays(30),     // Legacy: hardcoded 30-day payment terms, no customer config
                Amount = order.SubTotal - order.DiscountAmount,
                TaxAmount = order.TaxAmount,
                TotalAmount = order.TotalAmount,
                PaymentStatus = "Pending",
                CreatedAt = DateTime.Now
            };

            _db.Invoices.Add(invoice);
            _db.SaveChanges();

            return invoice;
        }

        // Legacy: overdue check using DateTime.Now without UTC normalization
        public bool IsOverdue(Invoice invoice)
        {
            return invoice.PaymentStatus == "Pending" && invoice.DueDate < DateTime.Now;
        }

        // Legacy: raw SQL for bulk overdue status update — not parameterized
        public void MarkOverdueInvoices()
        {
            var today = DateTime.Now.ToString("yyyy-MM-dd");
            var sql = "UPDATE Invoices SET PaymentStatus = 'Overdue' WHERE PaymentStatus = 'Pending' AND DueDate < '" + today + "'";
            _db.Database.ExecuteSqlCommand(sql);
        }
    }
}
