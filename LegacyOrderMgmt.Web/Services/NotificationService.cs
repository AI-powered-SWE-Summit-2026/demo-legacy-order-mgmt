using LegacyOrderMgmt.Core.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading;

namespace LegacyOrderMgmt.Web.Services
{
    public class NotificationService
    {
        private readonly IConfiguration _configuration;

        public NotificationService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Legacy: SmtpClient — obsolete, does not support modern auth (OAuth, XOAUTH2)
        // Should be replaced with MailKit or SendGrid/Azure Communication Services
        private SmtpClient CreateSmtpClient()
        {
            return new SmtpClient
            {
                Host = _configuration["Smtp:Host"],
                Port = int.Parse(_configuration["Smtp:Port"] ?? "25"),
                EnableSsl = bool.Parse(_configuration["Smtp:EnableSsl"] ?? "false"),
                Credentials = new NetworkCredential(
                    _configuration["Smtp:Username"],
                    _configuration["Smtp:Password"])
            };
        }

        public void SendOrderConfirmation(Order order)
        {
            try
            {
                using (var smtp = CreateSmtpClient())
                using (var msg = new MailMessage())
                {
                    msg.From = new MailAddress(_configuration["Smtp:FromAddress"]);
                    msg.To.Add(order.Customer.Email);
                    msg.Subject = $"Order Confirmation - {order.OrderNumber}";
                    msg.Body = BuildOrderConfirmationBody(order);
                    msg.IsBodyHtml = true;

                    // Legacy: SmtpClient.Send() is synchronous
                    smtp.Send(msg);
                }
            }
            catch (Exception ex)
            {
                // Legacy: swallowed exception — notification failure is silent
                Console.WriteLine($"[NotificationService] Email failed: {ex.Message}");
            }
        }

        public void SendShippingNotification(Order order)
        {
            var shipment = order.Shipments?.Count > 0 ? order.Shipments[order.Shipments.Count - 1] : null;
            if (shipment == null) return;

            try
            {
                using (var smtp = CreateSmtpClient())
                using (var msg = new MailMessage())
                {
                    msg.From = new MailAddress(_configuration["Smtp:FromAddress"]);
                    msg.To.Add(order.Customer.Email);
                    msg.Subject = $"Your order {order.OrderNumber} has shipped";
                    msg.Body = $"<p>Your order has been shipped via {shipment.Carrier}.<br/>Tracking: {shipment.TrackingNumber}</p>";
                    msg.IsBodyHtml = true;

                    // Legacy: retry with Thread.Sleep on transient failure
                    for (int i = 0; i < 3; i++)
                    {
                        try { smtp.Send(msg); return; }
                        catch { Thread.Sleep(2000); }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NotificationService] Shipping email failed: {ex.Message}");
            }
        }

        public void SendInvoiceToCustomer(Customer customer, Invoice invoice)
        {
            try
            {
                using (var smtp = CreateSmtpClient())
                using (var msg = new MailMessage())
                {
                    msg.From = new MailAddress(_configuration["Smtp:FromAddress"]);
                    msg.To.Add(customer.Email);
                    msg.Subject = $"Invoice {invoice.InvoiceNumber} - Due {invoice.DueDate:yyyy-MM-dd}";
                    msg.Body = $"<p>Please find your invoice {invoice.InvoiceNumber} for €{invoice.TotalAmount:F2} attached.</p>";
                    msg.IsBodyHtml = true;

                    smtp.Send(msg);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NotificationService] Invoice email failed: {ex.Message}");
            }
        }

        // Legacy: string concatenation to build HTML email body — no template engine
        private string BuildOrderConfirmationBody(Order order)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append("<h2>Order Confirmation</h2>");
            sb.Append($"<p>Order Number: <strong>{order.OrderNumber}</strong></p>");
            sb.Append($"<p>Date: {order.OrderDate:dd MMM yyyy}</p>");
            sb.Append("<table border='1'><tr><th>Product</th><th>Qty</th><th>Unit Price</th><th>Total</th></tr>");

            foreach (var line in order.Lines)
            {
                sb.Append($"<tr><td>{line.ProductName}</td><td>{line.Quantity}</td><td>€{line.UnitPrice:F2}</td><td>€{line.LineTotal:F2}</td></tr>");
            }

            sb.Append($"</table><p><strong>Total: €{order.TotalAmount:F2}</strong></p>");
            return sb.ToString();
        }
    }
}
