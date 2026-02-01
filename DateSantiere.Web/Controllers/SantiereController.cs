using DateSantiere.Data;
using DateSantiere.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DateSantiere.Web.Controllers;

public class SantiereController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SantiereController> _logger;

    public SantiereController(ApplicationDbContext context, ILogger<SantiereController> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IActionResult> Index(
        string? search, 
        string? judet, 
        string? categorie,
        string? status,
        int page = 1)
    {
        const int pageSize = 20;
        
        var query = _context.Santiere.Where(s => s.IsActive);

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(s => 
                s.Name.Contains(search) || 
                s.Description!.Contains(search) ||
                s.Beneficiar.Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(judet))
        {
            query = query.Where(s => s.Judet == judet);
        }

        if (!string.IsNullOrWhiteSpace(categorie))
        {
            query = query.Where(s => s.Categorie == categorie);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(s => s.Status == status);
        }

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var santiere = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.TotalCount = totalCount;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;
        ViewBag.Search = search;
        ViewBag.Judet = judet;
        ViewBag.Categorie = categorie;
        ViewBag.Status = status;

        return View(santiere);
    }

    public async Task<IActionResult> Details(int id)
    {
        var santier = await _context.Santiere
            .FirstOrDefaultAsync(s => s.Id == id && s.IsActive);

        if (santier == null)
        {
            return NotFound();
        }

        return View(santier);
    }

    [HttpGet]
    public async Task<IActionResult> GetJudete()
    {
        var judete = await _context.Santiere
            .Where(s => s.IsActive)
            .Select(s => s.Judet)
            .Distinct()
            .OrderBy(j => j)
            .ToListAsync();

        return Json(judete);
    }

    [HttpGet]
    public async Task<IActionResult> GetCategorii()
    {
        var categorii = await _context.Santiere
            .Where(s => s.IsActive)
            .Select(s => s.Categorie)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();

        return Json(categorii);
    }
}
