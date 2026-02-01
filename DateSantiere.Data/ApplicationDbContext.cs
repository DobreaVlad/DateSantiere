using DateSantiere.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DateSantiere.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Santier> Santiere { get; set; }
    public DbSet<SavedSearch> SavedSearches { get; set; }
    public DbSet<FavoriteSantier> FavoriteSantiere { get; set; }
    public DbSet<ContactRequest> ContactRequests { get; set; }
    public DbSet<Newsletter> Newsletters { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure relationships
        builder.Entity<SavedSearch>()
            .HasOne(s => s.User)
            .WithMany(u => u.SavedSearches)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<FavoriteSantier>()
            .HasOne(f => f.User)
            .WithMany(u => u.FavoriteSantiere)
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<FavoriteSantier>()
            .HasOne(f => f.Santier)
            .WithMany(s => s.FavoritedBy)
            .HasForeignKey(f => f.SantierId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes for better performance
        builder.Entity<Santier>()
            .HasIndex(s => s.Judet);

        builder.Entity<Santier>()
            .HasIndex(s => s.Categorie);

        builder.Entity<Santier>()
            .HasIndex(s => s.Status);

        builder.Entity<Santier>()
            .HasIndex(s => s.CreatedAt);

        builder.Entity<Newsletter>()
            .HasIndex(n => n.Email)
            .IsUnique();
    }
}
