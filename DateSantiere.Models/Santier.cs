using System.ComponentModel.DataAnnotations;

namespace DateSantiere.Models;

public class Santier
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(500)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(2000)]
    public string? Description { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Judet { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string Localitate { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Adresa { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Categorie { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string? Subcategorie { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Beneficiar { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string? BeneficiarCUI { get; set; }
    
    [StringLength(100)]
    public string? ContactPersoana { get; set; }
    
    [StringLength(100)]
    public string? ContactTelefon { get; set; }
    
    [StringLength(100)]
    public string? ContactEmail { get; set; }
    
    public decimal? ValoareEstimata { get; set; }
    
    [StringLength(50)]
    public string? Status { get; set; }
    
    public DateTime? DataIncepere { get; set; }
    
    public DateTime? DataFinalizare { get; set; }
    
    [StringLength(500)]
    public string? Proiectant { get; set; }
    
    [StringLength(500)]
    public string? Constructor { get; set; }
    
    [StringLength(2000)]
    public string? Observatii { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public bool IsActive { get; set; } = true;
    
    public bool IsFeatured { get; set; } = false;
    
    // Coordonate GPS
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    
    // Navigation properties
    public virtual ICollection<FavoriteSantier> FavoritedBy { get; set; } = new List<FavoriteSantier>();
}
