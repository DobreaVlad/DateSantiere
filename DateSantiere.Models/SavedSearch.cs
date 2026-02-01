using System.ComponentModel.DataAnnotations;

namespace DateSantiere.Models;

public class SavedSearch
{
    public int Id { get; set; }
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public string SearchParameters { get; set; } = string.Empty; // JSON
    
    public bool EmailNotifications { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual ApplicationUser User { get; set; } = null!;
}
