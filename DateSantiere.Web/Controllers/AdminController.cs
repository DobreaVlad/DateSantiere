using DateSantiere.Data;
using DateSantiere.Models;
using DateSantiere.Web.Services;
using DateSantiere.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using Hangfire;

namespace DateSantiere.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ApplicationDbContext _context;
    private readonly ScriptExecutionService _scriptExecutionService;
    
    public AdminController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ApplicationDbContext context,
        ScriptExecutionService scriptExecutionService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
        _scriptExecutionService = scriptExecutionService;
    }
    
    public async Task<IActionResult> Index()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        var permissions = AdminType.GetPermissions(currentUser?.AdminType);
        
        ViewBag.AdminType = currentUser?.AdminType;
        ViewBag.Permissions = permissions;
        
        return View();
    }

    public async Task<IActionResult> Applications()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        var permissions = AdminType.GetPermissions(currentUser?.AdminType);
        
        // Only SuperAdmin can access applications
        if (currentUser?.AdminType != AdminType.SuperAdmin)
        {
            return Forbid();
        }

        var scriptsPath = Path.Combine(Directory.GetCurrentDirectory(), "Scripts");
        var scripts = new List<ApplicationScript>();

        if (Directory.Exists(scriptsPath))
        {
            var files = Directory.GetFiles(scriptsPath, "*.ps1")
                .Union(Directory.GetFiles(scriptsPath, "*.bat"))
                .Union(Directory.GetFiles(scriptsPath, "*.sh"));

            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                scripts.Add(new ApplicationScript
                {
                    Name = Path.GetFileNameWithoutExtension(file),
                    FileName = fileInfo.Name,
                    Extension = fileInfo.Extension,
                    Path = file,
                    CreatedAt = fileInfo.CreationTime,
                    UpdatedAt = fileInfo.LastWriteTime,
                    Size = fileInfo.Length
                });
            }
        }

        return View(scripts.OrderByDescending(s => s.UpdatedAt).ToList());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RunScript(string fileName)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        
        // Only SuperAdmin can run scripts
        if (currentUser?.AdminType != AdminType.SuperAdmin)
        {
            return Forbid();
        }

        var scriptsPath = Path.Combine(Directory.GetCurrentDirectory(), "Scripts");
        var scriptFile = Path.Combine(scriptsPath, fileName);

        // Security check: ensure the script is within the Scripts directory
        if (!Path.GetFullPath(scriptFile).StartsWith(Path.GetFullPath(scriptsPath)))
        {
            TempData["Error"] = "Acces neautorizat la script.";
            return RedirectToAction(nameof(Applications));
        }

        if (!System.IO.File.Exists(scriptFile))
        {
            TempData["Error"] = "Scriptul nu a fost găsit.";
            return RedirectToAction(nameof(Applications));
        }

        try
        {
            // Queue the job in Hangfire
            var jobId = BackgroundJob.Enqueue(() => _scriptExecutionService.ExecuteScriptAsync(fileName));
            
            TempData["Success"] = $"Scriptul \"{Path.GetFileName(fileName)}\" a fost adăugat în coadă de execuție. Job ID: {jobId}";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Eroare la adăugarea scriptului în coadă: {ex.Message}";
        }

        return RedirectToAction(nameof(Applications));
    }

    public async Task<IActionResult> Users(string? search, string? accountType, string? adminType, bool? isActive)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        var permissions = AdminType.GetPermissions(currentUser?.AdminType);
        
        if (!permissions.CanManageUsers)
        {
            return Forbid();
        }
        
        // Start with all users
        var query = _context.Users.AsQueryable();
        
        // Apply search filter
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(u => 
                u.Email!.ToLower().Contains(search) ||
                (u.FirstName != null && u.FirstName.ToLower().Contains(search)) ||
                (u.LastName != null && u.LastName.ToLower().Contains(search)) ||
                (u.Company != null && u.Company.ToLower().Contains(search)));
        }
        
        // Apply account type filter
        if (!string.IsNullOrWhiteSpace(accountType))
        {
            query = query.Where(u => u.AccountType == accountType);
        }
        
        // Apply admin type filter
        if (!string.IsNullOrWhiteSpace(adminType))
        {
            if (adminType == "None")
            {
                query = query.Where(u => u.AdminType == null || u.AdminType == "");
            }
            else
            {
                query = query.Where(u => u.AdminType == adminType);
            }
        }
        
        // Apply active status filter
        if (isActive.HasValue)
        {
            query = query.Where(u => u.IsActive == isActive.Value);
        }
        
        var users = await query.OrderByDescending(u => u.CreatedAt).ToListAsync();
        
        // Get roles for each user
        var usersWithRoles = new List<UserWithRolesViewModel>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            usersWithRoles.Add(new UserWithRolesViewModel
            {
                User = user,
                Roles = roles.ToList()
            });
        }
        
        ViewBag.CanManageAdmins = permissions.CanManageAdmins;
        ViewBag.Search = search;
        ViewBag.AccountType = accountType;
        ViewBag.AdminType = adminType;
        ViewBag.IsActive = isActive;
        return View(usersWithRoles);
    }

    [HttpGet]
    public async Task<IActionResult> Reports(int page = 1, int pageSize = 25, string? search = null)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        var permissions = AdminType.GetPermissions(currentUser?.AdminType);
        if (!permissions.CanViewReports)
        {
            return Forbid();
        }

        // Base query: payment records with optional user email join
        var query = from p in _context.PaymentRecords
                    join u in _context.Users on p.UserId equals u.Id into uj
                    from u in uj.DefaultIfEmpty()
                    select new { p, UserEmail = u != null ? u.Email : "(unknown)" };

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(x => (x.UserEmail ?? "").ToLower().Contains(searchLower) || (x.p.StripeSessionId ?? "").ToLower().Contains(searchLower));
        }

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(x => x.p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new PaymentRowViewModel
            {
                Id = x.p.Id,
                UserEmail = x.UserEmail,
                Amount = x.p.Amount,
                Currency = x.p.Currency,
                Status = x.p.Status,
                CreatedAt = x.p.CreatedAt,
                StripeSessionId = x.p.StripeSessionId,
                SantierId = x.p.SantierId
            }).ToListAsync();

        var vm = new AdminReportsViewModel
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = total,
            Search = search
        };

        return View("ReportsNew", vm);
    }
    
    [HttpGet]
    public async Task<IActionResult> EditUser(string id)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        var permissions = AdminType.GetPermissions(currentUser?.AdminType);
        
        if (!permissions.CanManageUsers)
        {
            return Forbid();
        }
        
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }
        
        var userRoles = await _userManager.GetRolesAsync(user);
        var allRoles = await _roleManager.Roles.ToListAsync();
        
        var model = new EditUserViewModel
        {
            Id = user.Id,
            Email = user.Email ?? "",
            FirstName = user.FirstName,
            LastName = user.LastName,
            Company = user.Company,
            CUI = user.CUI,
            IsActive = user.IsActive,
            AccountType = user.AccountType,
            AdminType = user.AdminType,
            MonthlySearchLimit = user.MonthlySearchLimit,
            MonthlyExportLimit = user.MonthlyExportLimit,
            CurrentMonthSearches = user.CurrentMonthSearches,
            CurrentMonthExports = user.CurrentMonthExports,
            UserRoles = userRoles.ToList(),
            AllRoles = allRoles.Select(r => r.Name ?? "").ToList()
        };
        
        ViewBag.CanManageAdmins = permissions.CanManageAdmins;
        return View(model);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditUser(EditUserViewModel model)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        var permissions = AdminType.GetPermissions(currentUser?.AdminType);
        
        if (!permissions.CanManageUsers)
        {
            return Forbid();
        }
        
        var user = await _userManager.FindByIdAsync(model.Id);
        if (user == null)
        {
            return NotFound();
        }
        
        // Check if trying to edit admin without permission
        if (!permissions.CanManageAdmins && !string.IsNullOrEmpty(user.AdminType))
        {
            TempData["Error"] = "Nu ai permisiunea să modifici administratori.";
            return RedirectToAction(nameof(Users));
        }
        
        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        user.Company = model.Company;
        user.CUI = model.CUI;
        user.IsActive = model.IsActive;
        user.AccountType = model.AccountType;
        user.MonthlySearchLimit = model.MonthlySearchLimit;
        user.MonthlyExportLimit = model.MonthlyExportLimit;
        
        // Only SuperAdmin can change AdminType
        if (permissions.CanManageAdmins)
        {
            user.AdminType = model.AdminType;
        }
        
        var result = await _userManager.UpdateAsync(user);
        
        if (result.Succeeded)
        {
            TempData["Success"] = "Utilizatorul a fost actualizat cu succes.";
            return RedirectToAction(nameof(Users));
        }
        
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError("", error.Description);
        }
        
        return View(model);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        var permissions = AdminType.GetPermissions(currentUser?.AdminType);
        
        if (!permissions.CanManageUsers)
        {
            return Forbid();
        }
        
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }
        
        // Check if trying to delete admin without permission
        if (!permissions.CanManageAdmins && !string.IsNullOrEmpty(user.AdminType))
        {
            TempData["Error"] = "Nu ai permisiunea să ștergi administratori.";
            return RedirectToAction(nameof(Users));
        }
        
        // Prevent deleting yourself
        if (user.Id == currentUser?.Id)
        {
            TempData["Error"] = "Nu poți șterge propriul cont.";
            return RedirectToAction(nameof(Users));
        }
        
        var result = await _userManager.DeleteAsync(user);
        
        if (result.Succeeded)
        {
            TempData["Success"] = "Utilizatorul a fost șters cu succes.";
        }
        else
        {
            TempData["Error"] = "Eroare la ștergerea utilizatorului.";
        }
        
        return RedirectToAction(nameof(Users));
    }

    public async Task<IActionResult> Santiere(string? search, string? judet, string? categorie, string? status, bool? isActive, int page = 1)
    {
        const int pageSize = 20;

        // Start with all santiere (including inactive for admins)
        var query = _context.Santiere.AsQueryable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(s =>
                s.Name.ToLower().Contains(search) ||
                (s.Description != null && s.Description.ToLower().Contains(search)) ||
                s.Beneficiar.ToLower().Contains(search) ||
                (s.Localitate != null && s.Localitate.ToLower().Contains(search)) ||
                (s.Adresa != null && s.Adresa.ToLower().Contains(search)));
        }

        // Apply judet filter
        if (!string.IsNullOrWhiteSpace(judet))
        {
            query = query.Where(s => s.Judet == judet);
        }

        // Apply categorie filter
        if (!string.IsNullOrWhiteSpace(categorie))
        {
            query = query.Where(s => s.Categorie == categorie);
        }

        // Apply status filter
        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(s => s.Status == status);
        }

        // Apply active status filter
        if (isActive.HasValue)
        {
            query = query.Where(s => s.IsActive == isActive.Value);
        }

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var santiere = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Get distinct values for filters
        var judete = await _context.Santiere.Select(s => s.Judet).Distinct().OrderBy(j => j).ToListAsync();
        var categorii = await _context.Santiere.Select(s => s.Categorie).Distinct().OrderBy(c => c).ToListAsync();
        var statuses = await _context.Santiere.Where(s => s.Status != null).Select(s => s.Status).Distinct().OrderBy(st => st).ToListAsync();

        ViewBag.TotalCount = totalCount;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;
        ViewBag.Search = search;
        ViewBag.Judet = judet;
        ViewBag.Categorie = categorie;
        ViewBag.Status = status;
        ViewBag.IsActive = isActive;
        ViewBag.Judete = judete;
        ViewBag.Categorii = categorii;
        ViewBag.Statuses = statuses;

        return View(santiere);
    }

    [HttpGet]
    public async Task<IActionResult> EditSantiere(int id)
    {
        var santier = await _context.Santiere.FindAsync(id);
        if (santier == null)
        {
            return NotFound();
        }

        return View(santier);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditSantiere(int id, Santier santier)
    {
        if (id != santier.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                var existingSantier = await _context.Santiere.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
                if (existingSantier == null)
                {
                    return NotFound();
                }

                santier.UpdatedAt = DateTime.UtcNow;
                santier.CreatedAt = existingSantier.CreatedAt;
                
                _context.Update(santier);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Șantierul a fost actualizat cu succes.";
                return RedirectToAction(nameof(Santiere));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await SantierExists(santier.Id))
                {
                    return NotFound();
                }
                throw;
            }
        }

        return View(santier);
    }

    [HttpGet]
    public IActionResult CreateSantiere()
    {
        return View(new Santier());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateSantiere(Santier santier)
    {
        if (ModelState.IsValid)
        {
            santier.CreatedAt = DateTime.UtcNow;
            santier.UpdatedAt = DateTime.UtcNow;
            if (!santier.IsActive)
            {
                santier.IsActive = true;
            }

            _context.Add(santier);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Șantierul a fost creat cu succes.";
            return RedirectToAction(nameof(Santiere));
        }

        return View(santier);
    }

    [HttpGet]
    public async Task<IActionResult> ExportSantiere(string? search, string? judet, string? categorie, string? status, bool? isActive)
    {
        // Set EPPlus license context
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        // Start with all santiere (including inactive for admins)
        var query = _context.Santiere.AsQueryable();

        // Apply filters (same as Santiere action)
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(s =>
                s.Name.ToLower().Contains(search) ||
                (s.Description != null && s.Description.ToLower().Contains(search)) ||
                s.Beneficiar.ToLower().Contains(search) ||
                (s.Localitate != null && s.Localitate.ToLower().Contains(search)) ||
                (s.Adresa != null && s.Adresa.ToLower().Contains(search)));
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

        if (isActive.HasValue)
        {
            query = query.Where(s => s.IsActive == isActive.Value);
        }

        var santiere = await query.OrderByDescending(s => s.CreatedAt).ToListAsync();

        // Create Excel file
        using (var package = new ExcelPackage())
        {
            var worksheet = package.Workbook.Worksheets.Add("Șantiere");

            // Set column widths
            worksheet.Column(1).Width = 8;
            worksheet.Column(2).Width = 25;
            worksheet.Column(3).Width = 15;
            worksheet.Column(4).Width = 15;
            worksheet.Column(5).Width = 20;
            worksheet.Column(6).Width = 15;
            worksheet.Column(7).Width = 12;
            worksheet.Column(8).Width = 15;
            worksheet.Column(9).Width = 15;
            worksheet.Column(10).Width = 12;
            worksheet.Column(11).Width = 12;

            // Add header row
            var headerRow = worksheet.Row(1);
            headerRow.Style.Font.Bold = true;
            headerRow.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            headerRow.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(79, 129, 189));
            headerRow.Style.Font.Color.SetColor(System.Drawing.Color.White);

            var headers = new[] { "ID", "Nume", "Județ", "Localitate", "Beneficiar", "Categorie", "Status", "Valoare Est.", "Data Creare", "Latitudine", "Longitudine" };
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[1, i + 1].Value = headers[i];
            }

            // Add data rows
            for (int i = 0; i < santiere.Count; i++)
            {
                var santier = santiere[i];
                int row = i + 2;

                worksheet.Cells[row, 1].Value = santier.Id;
                worksheet.Cells[row, 2].Value = santier.Name;
                worksheet.Cells[row, 3].Value = santier.Judet;
                worksheet.Cells[row, 4].Value = santier.Localitate;
                worksheet.Cells[row, 5].Value = santier.Beneficiar;
                worksheet.Cells[row, 6].Value = santier.Categorie;
                worksheet.Cells[row, 7].Value = santier.Status ?? "-";
                worksheet.Cells[row, 8].Value = santier.ValoareEstimata.HasValue ? santier.ValoareEstimata.Value.ToString("N0") : "-";
                worksheet.Cells[row, 9].Value = santier.CreatedAt.ToString("dd.MM.yyyy");
                worksheet.Cells[row, 10].Value = santier.Latitude.HasValue ? santier.Latitude.Value.ToString("F6") : "-";
                worksheet.Cells[row, 11].Value = santier.Longitude.HasValue ? santier.Longitude.Value.ToString("F6") : "-";

                // Format number columns
                if (santier.ValoareEstimata.HasValue)
                {
                    worksheet.Cells[row, 8].Style.Numberformat.Format = "#,##0";
                }
            }

            // Freeze header row
            worksheet.View.FreezePanes(2, 1);

            // Auto-fit columns
            worksheet.Cells.AutoFitColumns();

            var fileName = $"Santiere_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.xlsx";
            var fileContent = package.GetAsByteArray();

            return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteSantiere(int id)
    {
        var santier = await _context.Santiere.FindAsync(id);
        if (santier == null)
        {
            return NotFound();
        }

        // Soft delete
        santier.IsActive = false;
        santier.UpdatedAt = DateTime.UtcNow;
        _context.Update(santier);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Șantierul a fost șters cu succes.";
        return RedirectToAction(nameof(Santiere));
    }

    private async Task<bool> SantierExists(int id)
    {
        return await _context.Santiere.AnyAsync(e => e.Id == id);
    }

    // API Key Settings Management
    public async Task<IActionResult> Settings()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        
        // Only SuperAdmin can access settings
        if (currentUser?.AdminType != AdminType.SuperAdmin)
        {
            return Forbid();
        }

        var settings = await _context.ApiKeySettings.ToListAsync();
        return View(settings);
    }

    public async Task<IActionResult> EditSetting(int id)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        
        if (currentUser?.AdminType != AdminType.SuperAdmin)
        {
            return Forbid();
        }

        var setting = await _context.ApiKeySettings.FindAsync(id);
        if (setting == null)
        {
            return NotFound();
        }

        return View(setting);
    }

    [HttpPost]
    public async Task<IActionResult> EditSetting(int id, ApiKeySettings model)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        
        if (currentUser?.AdminType != AdminType.SuperAdmin)
        {
            return Forbid();
        }

        var setting = await _context.ApiKeySettings.FindAsync(id);
        if (setting == null)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            setting.KeyName = model.KeyName;
            setting.Category = model.Category;
            setting.KeyValue = model.KeyValue;
            setting.Description = model.Description;
            setting.IsActive = model.IsActive;
            setting.UpdatedAt = DateTime.UtcNow;
            setting.UpdatedBy = currentUser?.Id;

            _context.Update(setting);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Setările au fost actualizate cu succes!";
            return RedirectToAction(nameof(Settings));
        }

        return View(setting);
    }

    public async Task<IActionResult> CreateSetting()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        
        if (currentUser?.AdminType != AdminType.SuperAdmin)
        {
            return Forbid();
        }

        return View("EditSetting", new ApiKeySettings());
    }

    [HttpPost]
    public async Task<IActionResult> CreateSetting(ApiKeySettings model)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        
        if (currentUser?.AdminType != AdminType.SuperAdmin)
        {
            return Forbid();
        }

        if (ModelState.IsValid)
        {
            model.CreatedAt = DateTime.UtcNow;
            model.UpdatedBy = currentUser?.Id;

            _context.Add(model);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Cheia a fost adăugată cu succes!";
            return RedirectToAction(nameof(Settings));
        }

        return View("EditSetting", model);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteSetting(int id)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        
        if (currentUser?.AdminType != AdminType.SuperAdmin)
        {
            return Forbid();
        }

        var setting = await _context.ApiKeySettings.FindAsync(id);
        if (setting == null)
        {
            return NotFound();
        }

        _context.Remove(setting);
        await _context.SaveChangesAsync();

        TempData["Message"] = "Cheia a fost ștearsă cu succes!";
        return RedirectToAction(nameof(Settings));
    }
}

public class UserWithRolesViewModel
{
    public ApplicationUser User { get; set; } = null!;
    public List<string> Roles { get; set; } = new();
}

public class EditUserViewModel
{
    public string Id { get; set; } = "";
    public string Email { get; set; } = "";
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Company { get; set; }
    public string? CUI { get; set; }
    public bool IsActive { get; set; }
    public string AccountType { get; set; } = "Free";
    public string? AdminType { get; set; }
    public int MonthlySearchLimit { get; set; }
    public int MonthlyExportLimit { get; set; }
    public int CurrentMonthSearches { get; set; }
    public int CurrentMonthExports { get; set; }
    public List<string> UserRoles { get; set; } = new();
    public List<string> AllRoles { get; set; } = new();
}

public class ApplicationScript
{
    public string Name { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public long Size { get; set; }
}
