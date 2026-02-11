using System.ComponentModel.DataAnnotations;

namespace DateSantiere.Models;

public class ApiKeySettings
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string KeyName { get; set; } = string.Empty;
    
    [StringLength(50)]
    public string? Category { get; set; } // Google, Stripe, Email, etc.
    
    [Required]
    [StringLength(1000)]
    public string KeyValue { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    public string? UpdatedBy { get; set; }
}
