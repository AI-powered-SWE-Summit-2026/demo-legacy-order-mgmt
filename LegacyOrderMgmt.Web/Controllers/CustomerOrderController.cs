using LegacyOrderMgmt.Core.Data;
using LegacyOrderMgmt.Core.Models;
using LegacyOrderMgmt.Core.Services;
using LegacyOrderMgmt.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace LegacyOrderMgmt.Web.Controllers
{
    // Legacy: mixes MVC Controller base with API-style JSON responses
    // Should use [ApiController] + ControllerBase for API endpoints
    public class CustomerOrderController : Controller
    {
        private readonly OrderDbContext _db;
        private readonly OrderService _orderService;
        private readonly PricingEngine _pricingEngine;
        private readonly EdiImportService _ediService;

        public CustomerOrderController(
            OrderDbContext db,
            OrderService orderService,
            PricingEngine pricingEngine,
            EdiImportService ediService)
        {
            _db = db;
            _orderService = orderService;
            _pricingEngine = pricingEngine;
            _ediService = ediService;
        }

        // Legacy: REST-ish endpoint on MVC controller — no [ApiController], no model validation
        [HttpPost]
        public IActionResult SubmitOrder([FromBody] dynamic payload)
        {
            if (payload == null)
                return BadRequest("No payload");

            int customerId = (int)payload.customerId;
            var customer = _db.Customers.Find(customerId);
            if (customer == null)
                return NotFound("Customer not found");

            var order = _orderService.CreateOrder(customerId, "api");

            foreach (var lineData in payload.lines)
            {
                int productId = (int)lineData.productId;
                int qty = (int)lineData.quantity;
                var product = _db.Products.Find(productId);
                if (product == null) continue;

                var discountPct = _pricingEngine.CalculateDiscount(order, customer.CreditLimit ?? "Standard");
                var line = new OrderLine
                {
                    OrderId = order.Id,
                    ProductId = productId,
                    ProductCode = product.ProductCode,
                    ProductName = product.Name,
                    Quantity = qty,
                    UnitPrice = product.UnitPrice,
                    DiscountPercent = discountPct,
                    LineTotal = _pricingEngine.CalculateLineTotal(qty, product.UnitPrice, discountPct)
                };
                _db.OrderLines.Add(line);
            }

            _db.SaveChanges();

            // Legacy: refreshes full order and recalculates — extra round trips
            order = _orderService.GetOrderById(order.Id);
            _pricingEngine.RecalculateOrderTotals(order, customer.CreditLimit ?? "Standard");
            _db.SaveChanges();

            // Legacy: uses Newtonsoft.Json explicitly instead of controller serialization
            return Content(JsonConvert.SerializeObject(new { orderId = order.Id, orderNumber = order.OrderNumber }), "application/json");
        }

        // Legacy: EDI import via synchronous XML parsing — blocks the request thread for large files
        [HttpPost]
        public IActionResult ImportEdi([FromBody] string xmlPayload)
        {
            if (string.IsNullOrEmpty(xmlPayload))
                return BadRequest("No EDI data");

            // Legacy: synchronous blocking call on EdiImportService
            var result = _ediService.ImportOrderFromXml(xmlPayload);
            if (!result.Success)
                return BadRequest(result.ErrorMessage);

            return Ok(new { imported = result.OrdersImported });
        }

        [HttpGet]
        public IActionResult CustomerOrders(int customerId)
        {
            var orders = _orderService.GetOrdersByCustomer(customerId);

            // Legacy: manual JSON serialization instead of using controller's built-in serializer
            var json = JsonConvert.SerializeObject(orders, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                DateFormatString = "yyyy-MM-dd"  // Legacy: hardcoded date format string
            });

            return Content(json, "application/json");
        }
    }
}
