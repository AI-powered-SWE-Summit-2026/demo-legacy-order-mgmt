using System;

namespace LegacyOrderMgmt.Core.Models
{
    [Serializable]
    public class Invoice
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; }

        public string InvoiceNumber { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal Amount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }

        // Legacy: payment status as magic strings instead of enum
        public string PaymentStatus { get; set; }  // "Pending","Paid","Overdue","Cancelled"
        public DateTime? PaidDate { get; set; }
        public string PaidBy { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
