using System.ComponentModel.DataAnnotations;

namespace DateSantiere.Models;

public class SantierNote
{
    public int Id { get; set; }
    
    [Required]
    public int SantierId { get; set; }
    public Santier? Santier { get; set; }
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }
    
    [Required(ErrorMessage = "Nota este obligatorie")]
    [StringLength(1000, ErrorMessage = "Nota nu poate avea mai mult de 1000 de caractere")]
    public string Nota { get; set; } = string.Empty;
    
    [Display(Name = "Data și ora alarmă")]
    public DateTime? Alarma { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public bool IsActive { get; set; } = true;
}
