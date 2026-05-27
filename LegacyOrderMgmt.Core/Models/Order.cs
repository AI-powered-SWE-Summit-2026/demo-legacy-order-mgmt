using System;
using System.Collections.Generic;

namespace LegacyOrderMgmt.Core.Models
{
    // Legacy: [Serializable] retained for BinaryFormatter-based order state caching
    [Serializable]
    public class Order
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; }
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }

        // Legacy: DateTime.Now used throughout instead of DateTimeOffset.UtcNow
        public DateTime OrderDate { get; set; }
        public DateTime? RequiredDate { get; set; }
        public DateTime? ShippedDate { get; set; }

        // Legacy: status stored as raw int instead of typed enum with value conversions
        public int Status { get; set; }           // 0=Draft,1=Confirmed,2=Processing,3=Shipped,4=Invoiced,5=Cancelled

        public string ShippingAddress { get; set; }
        public string ShippingCity { get; set; }
        public string ShippingCountry { get; set; }
        public string ShippingPostalCode { get; set; }

        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }

        public string Notes { get; set; }
        public string CreatedBy { get; set; }

        // Legacy: no audit trail abstraction — raw DateTime.Now stamps
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public List<OrderLine> Lines { get; set; } = new List<OrderLine>();
        public List<Invoice> Invoices { get; set; } = new List<Invoice>();
        public List<Shipment> Shipments { get; set; } = new List<Shipment>();
    }
}
