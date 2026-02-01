namespace DateSantiere.Models;

public class FavoriteSantier
{
    public int Id { get; set; }
    
    public string UserId { get; set; } = string.Empty;
    
    public int SantierId { get; set; }
    
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    
    public string? Notes { get; set; }
    
    // Navigation properties
    public virtual ApplicationUser User { get; set; } = null!;
    public virtual Santier Santier { get; set; } = null!;
}
