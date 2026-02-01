using DateSantiere.Data;
using DateSantiere.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DateSantiere.Web.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<HomeController> _logger;

    public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var stats = new
        {
            TotalSantiere = await _context.Santiere.CountAsync(s => s.IsActive),
            LastMonth = await _context.Santiere.CountAsync(s => 
                s.IsActive && s.CreatedAt >= DateTime.UtcNow.AddMonths(-1)),
            LastWeek = await _context.Santiere.CountAsync(s => 
                s.IsActive && s.CreatedAt >= DateTime.UtcNow.AddDays(-7))
        };
        
        ViewBag.Stats = stats;
        return View();
    }

    public IActionResult About()
    {
        return View();
    }

    public IActionResult Pricing()
    {
        return View();
    }

    public IActionResult Contact()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Contact(ContactRequest model)
    {
        if (ModelState.IsValid)
        {
            _context.ContactRequests.Add(model);
            await _context.SaveChangesAsync();
            
            TempData["Message"] = "Mesajul dumneavoastră a fost trimis cu succes!";
            return RedirectToAction(nameof(Contact));
        }
        
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Subscribe(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return Json(new { success = false, message = "Email-ul este obligatoriu" });
        }

        var existing = await _context.Newsletters.FirstOrDefaultAsync(n => n.Email == email);
        if (existing != null)
        {
            if (existing.IsActive)
            {
                return Json(new { success = false, message = "Acest email este deja abonat" });
            }
            else
            {
                existing.IsActive = true;
                existing.SubscribedAt = DateTime.UtcNow;
                existing.UnsubscribedAt = null;
            }
        }
        else
        {
            _context.Newsletters.Add(new Newsletter
            {
                Email = email,
                UnsubscribeToken = Guid.NewGuid().ToString()
            });
        }

        await _context.SaveChangesAsync();
        return Json(new { success = true, message = "V-ați abonat cu succes la newsletter!" });
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }
}
