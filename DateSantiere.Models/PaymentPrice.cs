using System;
using System.ComponentModel.DataAnnotations;

namespace DateSantiere.Models
{
    public class PaymentPrice
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        // Amount in cents
        public int AmountCents { get; set; }
        public string Currency { get; set; } = "eur";
        // "one-time", "monthly", "annual"
        public string BillingInterval { get; set; } = "one-time";
        public bool IsActive { get; set; } = true;
        // Mark if used for unlocking individual santier purchases
        public bool IsForSantier { get; set; } = false;
    }
}