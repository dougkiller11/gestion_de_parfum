using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Gestion_de_parfum.Data;
using Gestion_de_parfum.Models;

namespace Gestion_de_parfum.Controllers
{
    public class LigneCommandes1Controller : Controller
    {
        private readonly ApplicationDbContext _context;

        public LigneCommandes1Controller(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: LigneCommandes1
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.LigneCommandes.Include(l => l.Commande).Include(l => l.Parfum);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: LigneCommandes1/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ligneCommande = await _context.LigneCommandes
                .Include(l => l.Commande)
                .Include(l => l.Parfum)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ligneCommande == null)
            {
                return NotFound();
            }

            return View(ligneCommande);
        }

        // GET: LigneCommandes1/Create
        public IActionResult Create()
        {
            ViewData["CommandeId"] = new SelectList(_context.Commandes, "Id", "Id");
            ViewData["ParfumId"] = new SelectList(_context.Parfums, "Id", "Nom");
            return View();
        }

        // POST: LigneCommandes1/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Quantite,SousTotal,CommandeId,ParfumId")] LigneCommande ligneCommande)
        {
            if (ModelState.IsValid)
            {
                _context.Add(ligneCommande);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CommandeId"] = new SelectList(_context.Commandes, "Id", "Id", ligneCommande.CommandeId);
            ViewData["ParfumId"] = new SelectList(_context.Parfums, "Id", "Nom", ligneCommande.ParfumId);
            return View(ligneCommande);
        }

        // GET: LigneCommandes1/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ligneCommande = await _context.LigneCommandes.FindAsync(id);
            if (ligneCommande == null)
            {
                return NotFound();
            }
            ViewData["CommandeId"] = new SelectList(_context.Commandes, "Id", "Id", ligneCommande.CommandeId);
            ViewData["ParfumId"] = new SelectList(_context.Parfums, "Id", "Nom", ligneCommande.ParfumId);
            return View(ligneCommande);
        }

        // POST: LigneCommandes1/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Quantite,SousTotal,CommandeId,ParfumId")] LigneCommande ligneCommande)
        {
            if (id != ligneCommande.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ligneCommande);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LigneCommandeExists(ligneCommande.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CommandeId"] = new SelectList(_context.Commandes, "Id", "Id", ligneCommande.CommandeId);
            ViewData["ParfumId"] = new SelectList(_context.Parfums, "Id", "Nom", ligneCommande.ParfumId);
            return View(ligneCommande);
        }

        // GET: LigneCommandes1/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ligneCommande = await _context.LigneCommandes
                .Include(l => l.Commande)
                .Include(l => l.Parfum)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ligneCommande == null)
            {
                return NotFound();
            }

            return View(ligneCommande);
        }

        // POST: LigneCommandes1/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ligneCommande = await _context.LigneCommandes.FindAsync(id);
            if (ligneCommande != null)
            {
                _context.LigneCommandes.Remove(ligneCommande);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LigneCommandeExists(int id)
        {
            return _context.LigneCommandes.Any(e => e.Id == id);
        }
    }
}
