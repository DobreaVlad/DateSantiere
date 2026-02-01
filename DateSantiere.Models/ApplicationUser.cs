using Microsoft.AspNetCore.Identity;

namespace DateSantiere.Models;

public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Company { get; set; }
    public string? CUI { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public string? SubscriptionId { get; set; }
    public DateTime? SubscriptionEndDate { get; set; }
    public string? SubscriptionPlan { get; set; }
    
    // Navigation properties
    public virtual ICollection<SavedSearch> SavedSearches { get; set; } = new List<SavedSearch>();
    public virtual ICollection<FavoriteSantier> FavoriteSantiere { get; set; } = new List<FavoriteSantier>();
}
