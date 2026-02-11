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
    public DbSet<SantierNote> SantierNotes { get; set; }
    public DbSet<SantierHistory> SantiereHistory { get; set; }
    public DbSet<ApiKeySettings> ApiKeySettings { get; set; }
    public DbSet<DateSantiere.Models.PaymentRecord> PaymentRecords { get; set; }
    public DbSet<DateSantiere.Models.PurchasedSantier> PurchasedSantiere { get; set; }
    public DbSet<DateSantiere.Models.PaymentPrice> PaymentPrices { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Seed minimal default prices so admin can change later
        builder.Entity<DateSantiere.Models.PaymentPrice>().HasData(
            new DateSantiere.Models.PaymentPrice { Id = 1, Name = "Deblocare Șantier", AmountCents = 1499, Currency = "eur", BillingInterval = "one-time", IsActive = true, IsForSantier = true },
            new DateSantiere.Models.PaymentPrice { Id = 2, Name = "Pro Monthly", AmountCents = 999, Currency = "eur", BillingInterval = "monthly", IsActive = true, IsForSantier = false },
            new DateSantiere.Models.PaymentPrice { Id = 3, Name = "Pro Annual", AmountCents = 9999, Currency = "eur", BillingInterval = "annual", IsActive = true, IsForSantier = false }
        );
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

        builder.Entity<SantierNote>()
            .HasOne(sn => sn.Santier)
            .WithMany()
            .HasForeignKey(sn => sn.SantierId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<SantierNote>()
            .HasOne(sn => sn.User)
            .WithMany()
            .HasForeignKey(sn => sn.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<SantierNote>()
            .HasIndex(sn => new { sn.SantierId, sn.UserId });

        builder.Entity<SantierNote>()
            .HasIndex(sn => sn.Alarma);

        builder.Entity<SantierHistory>()
            .HasOne(sh => sh.Santier)
            .WithMany()
            .HasForeignKey(sh => sh.SantierId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<SantierHistory>()
            .HasOne(sh => sh.User)
            .WithMany()
            .HasForeignKey(sh => sh.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<SantierHistory>()
            .HasIndex(sh => new { sh.SantierId, sh.CreatedAt });

        builder.Entity<SantierHistory>()
            .HasIndex(sh => sh.CreatedAt);
    }
}
