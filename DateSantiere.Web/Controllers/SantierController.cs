using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DateSantiere.Data;
using DateSantiere.Models;

namespace DateSantiere.Web.Controllers;

[Authorize]
public class SantierController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public SantierController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: /Santier/Notes
    public async Task<IActionResult> Notes()
    {
        var user = await _userManager.GetUserAsync(User);
        var notes = await _context.SantierNotes
            .Include(n => n.Santier)
            .Where(n => n.UserId == user!.Id && n.IsActive)
            .OrderByDescending(n => n.Alarma ?? n.CreatedAt)
            .ToListAsync();

        return View(notes);
    }

    // GET: /Santier/CreateNota/SantierId=144758
    public async Task<IActionResult> CreateNota(int santierId)
    {
        var santier = await _context.Santiere.FindAsync(santierId);
        if (santier == null)
        {
            return NotFound();
        }

        ViewBag.Santier = santier;
        return View();
    }

    // POST: /Santier/CreateNota
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateNota(int santierId, string nota, DateTime? alarma)
    {
        var santier = await _context.Santiere.FindAsync(santierId);
        if (santier == null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(nota))
        {
            ViewBag.Santier = santier;
            ViewBag.Error = "Nota este obligatorie";
            return View();
        }

        if (!alarma.HasValue)
        {
            ViewBag.Santier = santier;
            ViewBag.Error = "Trebuie sa selectati o data";
            return View();
        }

        var user = await _userManager.GetUserAsync(User);
        var santierNote = new SantierNote
        {
            SantierId = santierId,
            UserId = user!.Id,
            Nota = nota,
            Alarma = alarma,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.SantierNotes.Add(santierNote);
        await _context.SaveChangesAsync();

        return RedirectToAction("Details", "Santiere", new { id = santierId });
    }

    // GET: /Santier/DeleteNota/144758
    public async Task<IActionResult> DeleteNota(int id)
    {
        var santier = await _context.Santiere.FindAsync(id);
        if (santier == null)
        {
            return NotFound();
        }

        var user = await _userManager.GetUserAsync(User);
        var note = await _context.SantierNotes
            .FirstOrDefaultAsync(n => n.SantierId == id && n.UserId == user!.Id && n.IsActive);

        if (note != null)
        {
            note.IsActive = false;
            note.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Details", "Santiere", new { id });
    }
}
