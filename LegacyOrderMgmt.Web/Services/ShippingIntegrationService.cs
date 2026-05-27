using LegacyOrderMgmt.Core.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Text;
using System.Threading;

namespace LegacyOrderMgmt.Web.Services
{
    public class ShippingIntegrationService
    {
        private readonly IConfiguration _configuration;

        public ShippingIntegrationService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Legacy: new HttpClient() instantiation — risk of socket exhaustion under load
        // Should use IHttpClientFactory
        public bool RegisterShipmentWithCarrier(string trackingNumber, string carrier, Order order)
        {
            var apiUrl = _configuration["ShippingApi:BaseUrl"];
            var apiKey = _configuration["ShippingApi:ApiKey"];

            var payload = new
            {
                tracking = trackingNumber,
                carrier = carrier,
                reference = order.OrderNumber,
                address = order.ShippingAddress,
                city = order.ShippingCity,
                country = order.ShippingCountry
            };

            var json = JsonConvert.SerializeObject(payload);

            // Legacy: new HttpClient() — not registered via IHttpClientFactory
            using (var client = new System.Net.Http.HttpClient())
            {
                client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);

                // Legacy: sync-over-async — .Result blocks the thread
                var response = client.PostAsync(
                    apiUrl + "/shipments",
                    new System.Net.Http.StringContent(json, Encoding.UTF8, "application/json")
                ).Result;

                return response.IsSuccessStatusCode;
            }
        }

        // Legacy: WebClient for carrier tracking lookups — deprecated type
        public string GetTrackingStatus(string trackingNumber, string carrier)
        {
            var apiUrl = _configuration["ShippingApi:BaseUrl"];
            var url = $"{apiUrl}/track/{carrier}/{trackingNumber}";

            try
            {
                // Legacy: WebClient usage — obsolete in modern .NET
                using (var wc = new WebClient())
                {
                    wc.Headers.Add("Accept", "application/json");
                    var result = wc.DownloadString(url);
                    dynamic parsed = JsonConvert.DeserializeObject(result);
                    return parsed?.status ?? "Unknown";
                }
            }
            catch
            {
                return "Unknown";
            }
        }

        // Legacy: retry loop with Thread.Sleep — blocks thread, no exponential backoff
        public bool NotifyWarehouse(int orderId, string warehouseCode)
        {
            var warehouseUrl = _configuration["WarehouseApi:BaseUrl"];

            for (int attempt = 1; attempt <= 3; attempt++)
            {
                try
                {
                    using (var client = new System.Net.Http.HttpClient())
                    {
                        // Legacy: sync-over-async
                        var response = client.PostAsync(
                            $"{warehouseUrl}/pick-requests",
                            new System.Net.Http.StringContent(
                                JsonConvert.SerializeObject(new { orderId, warehouseCode }),
                                Encoding.UTF8, "application/json")
                        ).Result;

                        if (response.IsSuccessStatusCode) return true;
                    }
                }
                catch { }

                // Legacy: Thread.Sleep retry delay — blocks thread
                Thread.Sleep(1000 * attempt);
            }

            return false;
        }
    }
}
