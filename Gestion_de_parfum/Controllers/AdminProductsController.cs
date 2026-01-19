using Gestion_de_parfum.Data;
using Gestion_de_parfum.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Gestion_de_parfum.Controllers;

[Authorize(Roles = "Admin")]
public class AdminProductsController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminProductsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var produits = await _context.Parfums.Include(p => p.Categorie).ToListAsync();
        return View(produits);
    }

    public async Task<IActionResult> Dashboard()
    {
        var totalProduits = await _context.Parfums.CountAsync();
        var totalCategories = await _context.Categories.CountAsync();
        var stockGlobal = await _context.Parfums.SumAsync(p => (int?)p.Stock) ?? 0;

        ViewBag.TotalProduits = totalProduits;
        ViewBag.TotalCategories = totalCategories;
        ViewBag.StockGlobal = stockGlobal;
        return View();
    }

    public async Task<IActionResult> Details(int id)
    {
        var p = await _context.Parfums.Include(x => x.Categorie).FirstOrDefaultAsync(x => x.Id == id);
        if (p == null) return NotFound();
        return View(p);
    }

    public IActionResult Create()
    {
        var categories = _context.Categories.AsNoTracking().ToList();
        ViewData["CategorieId"] = new SelectList(categories, "Id", "Nom");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Parfum parfum)
    {
        if (!ModelState.IsValid)
        {
            var categories = await _context.Categories.AsNoTracking().ToListAsync();
            ViewData["CategorieId"] = new SelectList(categories, "Id", "Nom", parfum.CategorieId);
            return View(parfum);
        }
        _context.Parfums.Add(parfum);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Produit créé.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var p = await _context.Parfums.FindAsync(id);
        if (p == null) return NotFound();
        var categories = await _context.Categories.AsNoTracking().ToListAsync();
        ViewData["CategorieId"] = new SelectList(categories, "Id", "Nom", p.CategorieId);
        return View(p);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Parfum parfum)
    {
        if (id != parfum.Id) return NotFound();
        if (!ModelState.IsValid)
        {
            var categories = await _context.Categories.AsNoTracking().ToListAsync();
            ViewData["CategorieId"] = new SelectList(categories, "Id", "Nom", parfum.CategorieId);
            return View(parfum);
        }

        _context.Update(parfum);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Produit mis à jour.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var p = await _context.Parfums.Include(x => x.Categorie).FirstOrDefaultAsync(x => x.Id == id);
        if (p == null) return NotFound();
        return View(p);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var p = await _context.Parfums.FindAsync(id);
        if (p != null)
        {
            _context.Parfums.Remove(p);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Produit supprimé.";
        }
        return RedirectToAction(nameof(Index));
    }
}


