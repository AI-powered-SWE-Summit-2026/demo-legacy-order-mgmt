using System;

namespace LegacyOrderMgmt.Core.Models
{
    [Serializable]
    public class OrderLine
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }

        public string ProductCode { get; set; }
        public string ProductName { get; set; }

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal LineTotal { get; set; }

        // Legacy: redundant status mirroring parent order — was added as a quick fix
        public int LineStatus { get; set; }
        public string Notes { get; set; }
    }
}
