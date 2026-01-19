using Gestion_de_parfum.Data;
using Gestion_de_parfum.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Gestion_de_parfum.Controllers;

// Public browsing allowed; actions that achetent devront vérifier l'authentification
public class BoutiqueController : Controller
{
    private readonly ApplicationDbContext _context;

    public BoutiqueController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? categorie = null, string? search = null)
    {
        var query = _context.Parfums
            .Include(p => p.Categorie)
            .AsQueryable();

        if (!string.IsNullOrEmpty(categorie))
        {
            query = query.Where(p => p.Categorie != null && p.Categorie.Nom == categorie);
        }

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(p => 
                p.Nom.Contains(search) || 
                (p.Marque != null && p.Marque.Contains(search)) ||
                (p.Description != null && p.Description.Contains(search))
            );
        }

        var parfums = await query.ToListAsync();
        var categories = await _context.Categories.ToListAsync();

        ViewBag.Categories = categories;
        ViewBag.SelectedCategorie = categorie;
        ViewBag.Search = search;

        return View(parfums);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var parfum = await _context.Parfums
            .Include(p => p.Categorie)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (parfum == null)
            return NotFound();

        return View(parfum);
    }
}

