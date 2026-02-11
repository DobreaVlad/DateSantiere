using System.Text.Json;
using DateSantiere.Models;
using Microsoft.EntityFrameworkCore;

namespace DateSantiere.Data;

public class SantierHistoryService
{
    private readonly ApplicationDbContext _context;

    public SantierHistoryService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task LogCreation(int santierId, string userId, string? ipAddress = null)
    {
        var santier = await _context.Santiere.FindAsync(santierId);
        if (santier == null) return;

        var history = new SantierHistory
        {
            SantierId = santierId,
            UserId = userId,
            Action = "Created",
            Changes = JsonSerializer.Serialize(new
            {
                santier.Name,
                santier.Judet,
                santier.Localitate,
                santier.Categorie,
                santier.ValoareEstimata,
                santier.Status
            }),
            IpAddress = ipAddress,
            CreatedAt = DateTime.UtcNow
        };

        _context.SantiereHistory.Add(history);
        await _context.SaveChangesAsync();
    }

    public async Task LogUpdate(int santierId, string userId, object oldValues, object newValues, string? ipAddress = null)
    {
        var changes = new
        {
            Before = oldValues,
            After = newValues
        };

        var history = new SantierHistory
        {
            SantierId = santierId,
            UserId = userId,
            Action = "Updated",
            Changes = JsonSerializer.Serialize(changes),
            IpAddress = ipAddress,
            CreatedAt = DateTime.UtcNow
        };

        _context.SantiereHistory.Add(history);
        await _context.SaveChangesAsync();
    }

    public async Task LogDeletion(int santierId, string userId, string? ipAddress = null)
    {
        var santier = await _context.Santiere.IgnoreQueryFilters().FirstOrDefaultAsync(s => s.Id == santierId);
        if (santier == null) return;

        var history = new SantierHistory
        {
            SantierId = santierId,
            UserId = userId,
            Action = "Deleted",
            Changes = JsonSerializer.Serialize(new
            {
                santier.Name,
                DeletedAt = DateTime.UtcNow
            }),
            IpAddress = ipAddress,
            CreatedAt = DateTime.UtcNow
        };

        _context.SantiereHistory.Add(history);
        await _context.SaveChangesAsync();
    }

    public async Task<List<SantierHistory>> GetHistory(int santierId)
    {
        return await _context.SantiereHistory
            .Include(h => h.User)
            .Where(h => h.SantierId == santierId)
            .OrderByDescending(h => h.CreatedAt)
            .ToListAsync();
    }
}
