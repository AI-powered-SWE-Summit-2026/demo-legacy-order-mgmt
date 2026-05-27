using LegacyOrderMgmt.Core.Data;
using LegacyOrderMgmt.Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace LegacyOrderMgmt.Web.Services
{
    public class EdiImportResult
    {
        public bool Success { get; set; }
        public int OrdersImported { get; set; }
        public string ErrorMessage { get; set; }
    }

    // Legacy EDI XML contract — tightly coupled to a specific customer's XML format
    [XmlRoot("PurchaseOrder")]
    public class EdiPurchaseOrder
    {
        [XmlElement("BuyerCode")] public string BuyerCode { get; set; }
        [XmlElement("OrderDate")] public string OrderDate { get; set; }
        [XmlArray("Lines")]
        [XmlArrayItem("Line")]
        public EdiOrderLine[] Lines { get; set; }
    }

    public class EdiOrderLine
    {
        [XmlElement("PartNumber")] public string PartNumber { get; set; }
        [XmlElement("Quantity")] public int Quantity { get; set; }
        [XmlElement("UnitPrice")] public decimal UnitPrice { get; set; }
    }

    public class EdiImportService
    {
        private readonly OrderDbContext _db;

        public EdiImportService(OrderDbContext db)
        {
            _db = db;
        }

        // Legacy: synchronous XmlSerializer-based EDI parsing
        // No streaming, no schema validation, entire payload loaded into memory
        public EdiImportResult ImportOrderFromXml(string xmlPayload)
        {
            EdiPurchaseOrder ediOrder;

            try
            {
                // Legacy: XmlSerializer — verbose, reflection-heavy, no support for modern JSON-over-HTTP EDI
                var serializer = new XmlSerializer(typeof(EdiPurchaseOrder));
                using (var reader = new StringReader(xmlPayload))
                {
                    ediOrder = (EdiPurchaseOrder)serializer.Deserialize(reader);
                }
            }
            catch (Exception ex)
            {
                return new EdiImportResult { Success = false, ErrorMessage = $"XML parse error: {ex.Message}" };
            }

            var customer = _db.Customers.FirstOrDefault(c => c.CustomerCode == ediOrder.BuyerCode);
            if (customer == null)
                return new EdiImportResult { Success = false, ErrorMessage = $"Unknown buyer code: {ediOrder.BuyerCode}" };

            // Legacy: DateTime.ParseExact with hardcoded format — breaks if customer changes date format
            DateTime orderDate;
            if (!DateTime.TryParseExact(ediOrder.OrderDate, "yyyyMMdd", null,
                    System.Globalization.DateTimeStyles.None, out orderDate))
            {
                orderDate = DateTime.Now;   // Legacy: silent fallback to now on parse failure
            }

            var order = new Order
            {
                CustomerId = customer.Id,
                OrderNumber = GenerateEdiOrderNumber(ediOrder.BuyerCode),
                OrderDate = orderDate,
                Status = 1,  // Confirmed immediately on EDI import
                CreatedBy = "edi-import",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            foreach (var ediLine in ediOrder.Lines ?? Array.Empty<EdiOrderLine>())
            {
                var product = _db.Products.FirstOrDefault(p => p.ProductCode == ediLine.PartNumber);
                if (product == null) continue;   // Legacy: silently skips unknown products

                order.Lines.Add(new OrderLine
                {
                    ProductId = product.Id,
                    ProductCode = product.ProductCode,
                    ProductName = product.Name,
                    Quantity = ediLine.Quantity,
                    UnitPrice = ediLine.UnitPrice > 0 ? ediLine.UnitPrice : product.UnitPrice,
                    DiscountPercent = 0,
                    LineTotal = ediLine.Quantity * (ediLine.UnitPrice > 0 ? ediLine.UnitPrice : product.UnitPrice)
                });
            }

            _db.Orders.Add(order);
            _db.SaveChanges();  // Legacy: synchronous SaveChanges

            return new EdiImportResult { Success = true, OrdersImported = 1 };
        }

        private string GenerateEdiOrderNumber(string buyerCode)
        {
            // Legacy: relies on MAX(Id) which has race condition under concurrent imports
            var lastOrder = _db.Orders.OrderByDescending(o => o.Id).FirstOrDefault();
            var nextId = (lastOrder?.Id ?? 0) + 1;
            return $"EDI-{buyerCode}-{DateTime.Now.Year}-{nextId:D5}";
        }
    }
}
