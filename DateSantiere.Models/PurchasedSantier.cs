using System;
using System.ComponentModel.DataAnnotations;

namespace DateSantiere.Models
{
    public class PurchasedSantier
    {
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int SantierId { get; set; }
        public DateTime PurchasedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiresAt { get; set; }
    }
}