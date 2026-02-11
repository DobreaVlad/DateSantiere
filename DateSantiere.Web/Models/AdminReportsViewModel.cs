using System;
using System.Collections.Generic;

namespace DateSantiere.Web.Models
{
    public class AdminReportsViewModel
    {
        public List<PaymentRowViewModel> Items { get; set; } = new List<PaymentRowViewModel>();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public string? Search { get; set; }
    }

    public class PaymentRowViewModel
    {
        public int Id { get; set; }
        public string? UserEmail { get; set; }
        public long Amount { get; set; }
        public string Currency { get; set; } = "eur";
        public string Status { get; set; } = "pending";
        public DateTime CreatedAt { get; set; }
        public string StripeSessionId { get; set; } = string.Empty;
        public int? SantierId { get; set; }
    }
}