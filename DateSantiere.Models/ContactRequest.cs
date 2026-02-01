using System.ComponentModel.DataAnnotations;

namespace DateSantiere.Models;

public class ContactRequest
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;
    
    [Phone]
    [StringLength(20)]
    public string? Phone { get; set; }
    
    [StringLength(200)]
    public string? Company { get; set; }
    
    [StringLength(50)]
    public string? CUI { get; set; }
    
    [Required]
    [StringLength(2000)]
    public string Message { get; set; } = string.Empty;
    
    [StringLength(50)]
    public string RequestType { get; set; } = "General"; // General, Quote, Demo, Support
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public bool IsProcessed { get; set; } = false;
    
    public DateTime? ProcessedAt { get; set; }
    
    public string? ProcessedBy { get; set; }
    
    public string? Response { get; set; }
}
