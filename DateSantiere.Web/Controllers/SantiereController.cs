using DateSantiere.Data;
using DateSantiere.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using OfficeOpenXml;

namespace DateSantiere.Web.Controllers;

public class SantiereController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SantiereController> _logger;
    private readonly SantierHistoryService _historyService;
    private readonly UserManager<ApplicationUser> _userManager;

    public SantiereController(
        ApplicationDbContext context, 
        ILogger<SantiereController> logger, 
        SantierHistoryService historyService,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _logger = logger;
        _historyService = historyService;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        string? search, 
        string? judet, 
        string? categorie,
        string? status,
        string? sort,
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

        // Apply sorting
        query = sort switch
        {
            "name-asc" => query.OrderBy(s => s.Name),
            "name-desc" => query.OrderByDescending(s => s.Name),
            "date-asc" => query.OrderBy(s => s.CreatedAt),
            "price-desc" => query.OrderByDescending(s => s.ValoareEstimata),
            "price-asc" => query.OrderBy(s => s.ValoareEstimata),
            _ => query.OrderByDescending(s => s.CreatedAt)
        };

        var santiere = await query
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
        ViewBag.Sort = sort;

        return View(santiere);
    }

    [HttpGet]
    public async Task<IActionResult> Export(
        string? search, 
        string? judet, 
        string? categorie,
        string? status,
        string? sort)
    {
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

        // Apply sorting
        query = sort switch
        {
            "name-asc" => query.OrderBy(s => s.Name),
            "name-desc" => query.OrderByDescending(s => s.Name),
            "date-asc" => query.OrderBy(s => s.CreatedAt),
            "price-desc" => query.OrderByDescending(s => s.ValoareEstimata),
            "price-asc" => query.OrderBy(s => s.ValoareEstimata),
            _ => query.OrderByDescending(s => s.CreatedAt)
        };

        var santiere = await query.ToListAsync();

        // Set EPPlus license context
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        // Generate Excel file
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Șantiere");

        // Add headers
        worksheet.Cells[1, 1].Value = "Nume";
        worksheet.Cells[1, 2].Value = "Descriere";
        worksheet.Cells[1, 3].Value = "Județ";
        worksheet.Cells[1, 4].Value = "Localitate";
        worksheet.Cells[1, 5].Value = "Categorie";
        worksheet.Cells[1, 6].Value = "Beneficiar";
        worksheet.Cells[1, 7].Value = "Valoare Estimată";
        worksheet.Cells[1, 8].Value = "Status";
        worksheet.Cells[1, 9].Value = "Data Începere";
        worksheet.Cells[1, 10].Value = "Data Finalizare";

        // Style headers
        using (var range = worksheet.Cells[1, 1, 1, 10])
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
        }

        // Add data
        int row = 2;
        foreach (var santier in santiere)
        {
            worksheet.Cells[row, 1].Value = santier.Name;
            worksheet.Cells[row, 2].Value = santier.Description;
            worksheet.Cells[row, 3].Value = santier.Judet;
            worksheet.Cells[row, 4].Value = santier.Localitate;
            worksheet.Cells[row, 5].Value = santier.Categorie;
            worksheet.Cells[row, 6].Value = santier.Beneficiar;
            worksheet.Cells[row, 7].Value = santier.ValoareEstimata;
            worksheet.Cells[row, 8].Value = santier.Status;
            worksheet.Cells[row, 9].Value = santier.DataIncepere?.ToString("dd.MM.yyyy");
            worksheet.Cells[row, 10].Value = santier.DataFinalizare?.ToString("dd.MM.yyyy");
            row++;
        }

        // Auto-fit columns
        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

        var bytes = package.GetAsByteArray();
        var fileName = $"santiere_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    public async Task<IActionResult> Details(int id)
    {
        var santier = await _context.Santiere
            .FirstOrDefaultAsync(s => s.Id == id && s.IsActive);

        if (santier == null)
        {
            return NotFound();
        }

        bool hasAccess = false;
        bool limitReached = false;

        if (User.Identity?.IsAuthenticated == true)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                // Reset monthly counters if needed
                if (user.LastResetDate.Month != DateTime.UtcNow.Month || user.LastResetDate.Year != DateTime.UtcNow.Year)
                {
                    user.CurrentMonthSearches = 0;
                    user.CurrentMonthExports = 0;
                    user.LastResetDate = DateTime.UtcNow;
                }

                // Check subscription
                if (user.SubscriptionEndDate != null && user.SubscriptionEndDate > DateTime.UtcNow)
                {
                    hasAccess = true;
                }

                // Check per-santier purchase
                var purchased = await _context.PurchasedSantiere.FirstOrDefaultAsync(p => p.UserId == user.Id && p.SantierId == santier.Id && (p.ExpiresAt == null || p.ExpiresAt > DateTime.UtcNow));
                if (purchased != null)
                {
                    hasAccess = true;
                }

                // Increment searches and enforce limit if no subscription
                if (!hasAccess)
                {
                    if (user.MonthlySearchLimit > 0 && user.CurrentMonthSearches >= user.MonthlySearchLimit)
                    {
                        limitReached = true;
                    }
                    else
                    {
                        user.CurrentMonthSearches += 1;
                    }
                }

                _context.Users.Update(user);
                await _context.SaveChangesAsync();
            }
        }

        ViewBag.HasAccess = hasAccess;
        ViewBag.LimitReached = limitReached;

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

    [HttpGet]
    public async Task<IActionResult> GetSantiereMap(string? search, string? judet, string? categorie, string? status)
    {
        var query = _context.Santiere.Where(s => s.IsActive && s.Latitude.HasValue && s.Longitude.HasValue);

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

        var santiere = await query
            .Select(s => new {
                s.Id,
                s.Name,
                s.Judet,
                s.Localitate,
                s.Categorie,
                s.Beneficiar,
                s.Latitude,
                s.Longitude,
                s.ValoareEstimata
            })
            .ToListAsync();

        return Json(santiere);
    }

    // GET: Santiere/History/5
    public async Task<IActionResult> History(int id)
    {
        var santier = await _context.Santiere
            .FirstOrDefaultAsync(s => s.Id == id && s.IsActive);
            
        if (santier == null)
        {
            return NotFound();
        }

        var history = await _historyService.GetHistory(id);
        ViewBag.Santier = santier;
        
        return View(history);
    }

    // GET: Santiere/Create
    [Authorize(Roles = "Admin")]
    public IActionResult Create()
    {
        return View();
    }

    // POST: Santiere/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(Santier santier)
    {
        if (ModelState.IsValid)
        {
            santier.CreatedAt = DateTime.UtcNow;
            santier.UpdatedAt = DateTime.UtcNow;
            santier.IsActive = true;

            _context.Add(santier);
            await _context.SaveChangesAsync();

            // Log creation
            var user = await _userManager.GetUserAsync(User);
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            await _historyService.LogCreation(santier.Id, user!.Id, ipAddress);

            TempData["Success"] = "Șantierul a fost creat cu succes!";
            return RedirectToAction(nameof(Details), new { id = santier.Id });
        }
        return View(santier);
    }

    // GET: Santiere/Edit/5
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        var santier = await _context.Santiere
            .FirstOrDefaultAsync(s => s.Id == id && s.IsActive);

        if (santier == null)
        {
            return NotFound();
        }

        return View(santier);
    }

    // POST: Santiere/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id, Santier santier)
    {
        if (id != santier.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                // Capture old values before update
                var oldSantier = await _context.Santiere.AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (oldSantier == null)
                {
                    return NotFound();
                }

                santier.UpdatedAt = DateTime.UtcNow;
                _context.Update(santier);
                await _context.SaveChangesAsync();

                // Log update with before/after values
                var user = await _userManager.GetUserAsync(User);
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                
                var changes = new Dictionary<string, object>();
                var properties = typeof(Santier).GetProperties();
                
                foreach (var prop in properties)
                {
                    if (prop.Name == "UpdatedAt" || prop.Name == "CreatedAt") continue;
                    
                    var oldValue = prop.GetValue(oldSantier);
                    var newValue = prop.GetValue(santier);
                    
                    if (!Equals(oldValue, newValue))
                    {
                        changes[prop.Name] = new { Before = oldValue, After = newValue };
                    }
                }

                if (changes.Count > 0)
                {
                    await _historyService.LogUpdate(id, user!.Id, changes, changes, ipAddress);
                }

                TempData["Success"] = "Șantierul a fost actualizat cu succes!";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await SantierExists(santier.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Details), new { id = santier.Id });
        }
        return View(santier);
    }

    // GET: Santiere/Delete/5
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var santier = await _context.Santiere
            .FirstOrDefaultAsync(s => s.Id == id && s.IsActive);

        if (santier == null)
        {
            return NotFound();
        }

        return View(santier);
    }

    // POST: Santiere/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var santier = await _context.Santiere.FindAsync(id);
        
        if (santier != null)
        {
            // Soft delete
            santier.IsActive = false;
            santier.UpdatedAt = DateTime.UtcNow;
            _context.Update(santier);
            await _context.SaveChangesAsync();

            // Log deletion
            var user = await _userManager.GetUserAsync(User);
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            await _historyService.LogDeletion(id, user!.Id, ipAddress);

            TempData["Success"] = "Șantierul a fost șters cu succes!";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<bool> SantierExists(int id)
    {
        return await _context.Santiere.AnyAsync(e => e.Id == id && e.IsActive);
    }
}
