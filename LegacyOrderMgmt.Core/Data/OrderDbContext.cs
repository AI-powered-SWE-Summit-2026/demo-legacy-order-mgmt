using LegacyOrderMgmt.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace LegacyOrderMgmt.Core.Data
{
    public class OrderDbContext : DbContext
    {
        public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options) { }

        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderLine> OrderLines { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<Shipment> Shipments { get; set; }
        public DbSet<PricingRule> PricingRules { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Legacy: no Fluent API conventions — minimal configuration, relies on EF defaults
            modelBuilder.Entity<Order>()
                .HasMany(o => o.Lines)
                .WithOne(l => l.Order)
                .HasForeignKey(l => l.OrderId);

            modelBuilder.Entity<Order>()
                .HasMany(o => o.Invoices)
                .WithOne(i => i.Order)
                .HasForeignKey(i => i.OrderId);

            modelBuilder.Entity<Order>()
                .HasMany(o => o.Shipments)
                .WithOne(s => s.Order)
                .HasForeignKey(s => s.OrderId);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId);

            // Legacy: no decimal precision configured — relies on SQL Server defaults
            // This causes rounding issues with currency values
        }
    }
}
