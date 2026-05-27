using System;

namespace LegacyOrderMgmt.Core.Models
{
    public class PricingRule
    {
        public int Id { get; set; }
        public string RuleName { get; set; }

        // Legacy: customer tier stored as raw string — no foreign key or lookup
        public string CustomerTier { get; set; }  // "Standard","Silver","Gold","Platinum"
        public int? ProductCategoryId { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal? MinOrderValue { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public bool IsActive { get; set; }
    }
}
