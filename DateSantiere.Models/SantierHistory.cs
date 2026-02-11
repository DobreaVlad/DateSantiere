using System.ComponentModel.DataAnnotations;

namespace DateSantiere.Models;

public class SantierHistory
{
    public int Id { get; set; }
    
    [Required]
    public int SantierId { get; set; }
    public Santier? Santier { get; set; }
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }
    
    [Required]
    public string Action { get; set; } = string.Empty; // "Created", "Updated", "Deleted"
    
    [Required]
    public string Changes { get; set; } = string.Empty; // JSON cu modificările
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public string? IpAddress { get; set; }
}
