using System;

namespace LegacyOrderMgmt.Core.Models
{
    public class Shipment
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; }

        public string TrackingNumber { get; set; }
        public string Carrier { get; set; }
        public DateTime ShipDate { get; set; }
        public DateTime? EstimatedDelivery { get; set; }
        public DateTime? ActualDelivery { get; set; }

        // Legacy: status as int magic number — no enum or lookup table
        public int ShipmentStatus { get; set; }  // 0=Pending,1=InTransit,2=Delivered,3=Failed

        public string ShippingAddress { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
