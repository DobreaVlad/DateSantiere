using System;
using System.ComponentModel.DataAnnotations;

namespace DateSantiere.Models
{
    public class PaymentRecord
    {
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int? SantierId { get; set; }
        public int? PriceId { get; set; }
        public long Amount { get; set; }
        public string Currency { get; set; } = "eur";
        public string StripeSessionId { get; set; } = string.Empty;
        public string StripePaymentIntentId { get; set; } = string.Empty;
        public string Status { get; set; } = "pending";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}