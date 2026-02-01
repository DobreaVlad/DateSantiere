using System.ComponentModel.DataAnnotations;

namespace DateSantiere.Models;

public class Newsletter
{
    public int Id { get; set; }
    
    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string? Name { get; set; }
    
    public DateTime SubscribedAt { get; set; } = DateTime.UtcNow;
    
    public bool IsActive { get; set; } = true;
    
    public string? UnsubscribeToken { get; set; }
    
    public DateTime? UnsubscribedAt { get; set; }
}
