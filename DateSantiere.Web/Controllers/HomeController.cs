using DateSantiere.Data;
using DateSantiere.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Text.Json;

namespace DateSantiere.Web.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<HomeController> _logger;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public HomeController(ApplicationDbContext context, ILogger<HomeController> logger, IConfiguration configuration, HttpClient httpClient)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
        _httpClient = httpClient;
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
        ViewData["IsHome"] = true;
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
    public async Task<IActionResult> Contact(ContactRequest model, string recaptchaToken)
    {
        // Validate reCAPTCHA
        if (string.IsNullOrWhiteSpace(recaptchaToken))
        {
            ModelState.AddModelError("", "reCAPTCHA validation failed. Please try again.");
            return View(model);
        }

        var isValidRecaptcha = await ValidateRecaptchaToken(recaptchaToken);
        if (!isValidRecaptcha)
        {
            ModelState.AddModelError("", "reCAPTCHA validation failed. Please try again.");
            return View(model);
        }

        if (ModelState.IsValid)
        {
            _context.ContactRequests.Add(model);
            await _context.SaveChangesAsync();
            
            TempData["Message"] = "Mesajul dumneavoastră a fost trimis cu succes!";
            return RedirectToAction(nameof(Contact));
        }
        
        return View(model);
    }

    private async Task<bool> ValidateRecaptchaToken(string token)
    {
        try
        {
            var secretKey = _configuration["GoogleRecaptcha:SecretKey"];
            if (string.IsNullOrWhiteSpace(secretKey))
            {
                _logger.LogWarning("Google reCAPTCHA secret key not configured");
                return true; // Allow if not configured
            }

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("secret", secretKey),
                new KeyValuePair<string, string>("response", token)
            });

            var response = await _httpClient.PostAsync("https://www.google.com/recaptcha/api/siteverify", content);
            var responseBody = await response.Content.ReadAsStringAsync();

            using (JsonDocument doc = JsonDocument.Parse(responseBody))
            {
                var root = doc.RootElement;
                
                if (root.TryGetProperty("success", out var successProperty) && successProperty.GetBoolean())
                {
                    if (root.TryGetProperty("score", out var scoreProperty))
                    {
                        var score = scoreProperty.GetDouble();
                        // Accept scores above 0.5 (0.0 is most likely bot, 1.0 is most likely human)
                        return score >= 0.5;
                    }
                    return true;
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating reCAPTCHA token");
            return false;
        }
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
