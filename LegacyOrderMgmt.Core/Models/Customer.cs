using System;
using System.Collections.Generic;

namespace LegacyOrderMgmt.Core.Models
{
    public class Customer
    {
        public int Id { get; set; }
        public string CustomerCode { get; set; }
        public string CompanyName { get; set; }
        public string ContactName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string BillingAddress { get; set; }
        public string BillingCity { get; set; }
        public string BillingCountry { get; set; }
        public string BillingPostalCode { get; set; }
        public string CreditLimit { get; set; }   // Legacy: stored as string, never cast to decimal safely
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        public List<Order> Orders { get; set; } = new List<Order>();
    }
}
