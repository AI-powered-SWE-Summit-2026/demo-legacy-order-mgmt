using LegacyOrderMgmt.Core.Data;
using LegacyOrderMgmt.Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace LegacyOrderMgmt.Core.Services
{
    public class PricingEngine
    {
        private readonly OrderDbContext _db;

        // Legacy: static in-memory cache of rules with no expiry or invalidation strategy
        private static List<PricingRule> _cachedRules;
        private static DateTime _cacheLoadedAt = DateTime.MinValue;

        public PricingEngine(OrderDbContext db)
        {
            _db = db;
        }

        public decimal CalculateDiscount(Order order, string customerTier)
        {
            var rules = LoadRules();

            // Legacy: DateTime.Now for validity check instead of UTC
            var now = DateTime.Now;

            var applicableRules = rules
                .Where(r => r.IsActive
                    && r.CustomerTier == customerTier
                    && r.ValidFrom <= now
                    && (r.ValidTo == null || r.ValidTo >= now)
                    && (r.MinOrderValue == null || order.SubTotal >= r.MinOrderValue))
                .OrderByDescending(r => r.DiscountPercent)
                .ToList();

            if (!applicableRules.Any())
                return 0;

            // Legacy: takes highest single rule — no stacking, no complex rule evaluation
            return applicableRules.First().DiscountPercent;
        }

        public decimal CalculateLineTotal(int quantity, decimal unitPrice, decimal discountPercent)
        {
            // Legacy: decimal precision issues — no rounding strategy defined
            var lineTotal = quantity * unitPrice;
            var discount = lineTotal * (discountPercent / 100);
            return lineTotal - discount;
        }

        public void RecalculateOrderTotals(Order order, string customerTier)
        {
            // Legacy: synchronous loop over lines with no bulk update
            foreach (var line in order.Lines)
            {
                line.LineTotal = CalculateLineTotal(line.Quantity, line.UnitPrice, line.DiscountPercent);
            }

            order.SubTotal = order.Lines.Sum(l => l.LineTotal);
            var discountPct = CalculateDiscount(order, customerTier);
            order.DiscountAmount = order.SubTotal * (discountPct / 100);

            // Legacy: hardcoded 20% VAT — no configuration, no multi-country tax handling
            order.TaxAmount = (order.SubTotal - order.DiscountAmount) * 0.20m;
            order.TotalAmount = order.SubTotal - order.DiscountAmount + order.TaxAmount;

            order.UpdatedAt = DateTime.Now;
        }

        // Legacy: 5-minute cache with no invalidation on rule change
        private List<PricingRule> LoadRules()
        {
            if (_cachedRules != null && (DateTime.Now - _cacheLoadedAt).TotalMinutes < 5)
                return _cachedRules;

            // Legacy: Thread.Sleep as a fake "load delay" retained from original dev testing
            Thread.Sleep(50);

            _cachedRules = _db.PricingRules.ToList();
            _cacheLoadedAt = DateTime.Now;
            return _cachedRules;
        }
    }
}
