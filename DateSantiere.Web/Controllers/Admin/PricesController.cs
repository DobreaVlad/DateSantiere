using DateSantiere.Data;
using DateSantiere.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DateSantiere.Web.Controllers.Admin;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class PricesController : Controller
{
    private readonly ApplicationDbContext _db;

    public PricesController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var prices = await _db.PaymentPrices.OrderBy(p => p.Id).ToListAsync();
        return View(prices);
    }

    public IActionResult Create()
    {
        return View(new PaymentPrice());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PaymentPrice price)
    {
        if (ModelState.IsValid)
        {
            _db.PaymentPrices.Add(price);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        return View(price);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var price = await _db.PaymentPrices.FindAsync(id);
        if (price == null) return NotFound();
        return View(price);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(PaymentPrice price)
    {
        if (ModelState.IsValid)
        {
            _db.PaymentPrices.Update(price);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        return View(price);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var price = await _db.PaymentPrices.FindAsync(id);
        if (price == null) return NotFound();
        _db.PaymentPrices.Remove(price);
        await _db.SaveChangesAsync();
        return RedirectToAction("Index");
    }
}