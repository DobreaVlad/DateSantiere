using DateSantiere.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DateSantiere.Web.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        ILogger<AccountController> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string email, string password, bool rememberMe, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (ModelState.IsValid)
        {
            var result = await _signInManager.PasswordSignInAsync(email, password, rememberMe, lockoutOnFailure: false);
            
            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in.");
                
                // Check if it's an AJAX request
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, redirectUrl = returnUrl ?? Url.Action("Index", "Home") });
                }
                
                return LocalRedirect(returnUrl ?? Url.Action("Index", "Home")!);
            }
            if (result.RequiresTwoFactor)
            {
                return RedirectToAction(nameof(LoginWith2fa), new { returnUrl, rememberMe });
            }
            if (result.IsLockedOut)
            {
                _logger.LogWarning("User account locked out.");
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, error = "Contul este blocat." });
                }
                
                return View("Lockout");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Încercare de autentificare invalidă.");
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, error = "Email sau parolă greșită." });
                }
                
                return View();
            }
        }

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return Json(new { success = false, error = "Date invalide." });
        }

        return View();
    }

    [HttpGet]
    public IActionResult Register(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(
        string email, 
        string password, 
        string confirmPassword,
        string? firstName,
        string? lastName,
        bool rememberMe = false,
        string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (password != confirmPassword)
        {
            ModelState.AddModelError(string.Empty, "Parolele nu se potrivesc.");
            
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = false, error = "Parolele nu se potrivesc." });
            }
            
            return View();
        }

        if (ModelState.IsValid)
        {
            var user = new ApplicationUser 
            { 
                UserName = email, 
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                AccountType = AccountType.Free,
                MonthlySearchLimit = 10,
                MonthlyExportLimit = 0,
                CurrentMonthSearches = 0,
                CurrentMonthExports = 0,
                LastResetDate = DateTime.UtcNow
            };
            
            var result = await _userManager.CreateAsync(user, password);
            
            if (result.Succeeded)
            {
                _logger.LogInformation("User created a new account with password.");

                await _userManager.AddToRoleAsync(user, "User");

                await _signInManager.SignInAsync(user, isPersistent: rememberMe);
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, redirectUrl = returnUrl ?? Url.Action("Index", "Home") });
                }
                
                return LocalRedirect(returnUrl ?? Url.Action("Index", "Home")!);
            }
            
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = false, error = errors });
            }
            
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return Json(new { success = false, error = "Date invalide." });
        }

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        _logger.LogInformation("User logged out.");
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = false, error = "Email este obligatoriu." });
            }
            return BadRequest();
        }

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            // Don't reveal that the user does not exist
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, message = "Dacă emailul există, vei primi un link de resetare." });
            }
            return RedirectToAction("ForgotPasswordConfirmation");
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var callbackUrl = Url.Action("ResetPassword", "Account", new { token, email = user.Email }, Request.Scheme);

        // TODO: Send email with callbackUrl
        _logger.LogInformation($"Password reset requested for {email}. Reset URL: {callbackUrl}");

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return Json(new { success = true, message = "Link de resetare trimis pe email!" });
        }

        return RedirectToAction("ForgotPasswordConfirmation");
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        return View(user);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ApplicationUser model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        // Update user properties
        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        user.Company = model.Company;
        user.CUI = model.CUI;

        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            // Reload user from database to get fresh data
            user = await _userManager.FindByIdAsync(user.Id);
            
            // Sign out and sign back in to refresh the authentication cookie with updated claims
            await _signInManager.SignOutAsync();
            await _signInManager.SignInAsync(user, isPersistent: false);
            
            TempData["SuccessMessage"] = "Profil actualizat cu succes!";
            _logger.LogInformation("User updated their profile.");
        }
        else
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        return View(user);
    }

    private IActionResult LocalRedirect(string returnUrl)
    {
        if (Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        else
        {
            return RedirectToAction("Index", "Home");
        }
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        else
        {
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }
    }

    [HttpGet]
    public async Task<IActionResult> LoginWith2fa(bool rememberMe, string? returnUrl = null)
    {
        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

        if (user == null)
        {
            throw new InvalidOperationException("Unable to load two-factor authentication user.");
        }

        ViewData["ReturnUrl"] = returnUrl;
        ViewData["RememberMe"] = rememberMe;

        return View();
    }
}
