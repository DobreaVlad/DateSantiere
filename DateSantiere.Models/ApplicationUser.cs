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
    
    // Admin Type: null (regular user), "SuperAdmin", "Admin", "Moderator", "Support"
    public string? AdminType { get; set; }
    
    // Account Type: Free, Basic, Premium, Enterprise
    public string AccountType { get; set; } = "Free";
    public string? SubscriptionId { get; set; }
    public DateTime? SubscriptionEndDate { get; set; }
    public string? SubscriptionPlan { get; set; }
    
    // Access limits based on account type
    public int MonthlySearchLimit { get; set; } = 10; // Free: 10, Basic: 100, Premium: 500, Enterprise: Unlimited
    public int MonthlyExportLimit { get; set; } = 0;  // Free: 0, Basic: 10, Premium: 50, Enterprise: Unlimited
    public int CurrentMonthSearches { get; set; } = 0;
    public int CurrentMonthExports { get; set; } = 0;
    public DateTime LastResetDate { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual ICollection<SavedSearch> SavedSearches { get; set; } = new List<SavedSearch>();
    public virtual ICollection<FavoriteSantier> FavoriteSantiere { get; set; } = new List<FavoriteSantier>();
}
