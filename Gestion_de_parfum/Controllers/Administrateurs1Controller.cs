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
    public class Administrateurs1Controller : Controller
    {
        private readonly ApplicationDbContext _context;

        public Administrateurs1Controller(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Administrateurs1
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Administrateurs.Include(a => a.Utilisateur);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Administrateurs1/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var administrateur = await _context.Administrateurs
                .Include(a => a.Utilisateur)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (administrateur == null)
            {
                return NotFound();
            }

            return View(administrateur);
        }

        // GET: Administrateurs1/Create
        public IActionResult Create()
        {
            ViewData["Id"] = new SelectList(_context.Utilisateurs, "Id", "Email");
            return View();
        }

        // POST: Administrateurs1/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id")] Administrateur administrateur)
        {
            if (ModelState.IsValid)
            {
                _context.Add(administrateur);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Id"] = new SelectList(_context.Utilisateurs, "Id", "Email", administrateur.Id);
            return View(administrateur);
        }

        // GET: Administrateurs1/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var administrateur = await _context.Administrateurs.FindAsync(id);
            if (administrateur == null)
            {
                return NotFound();
            }
            ViewData["Id"] = new SelectList(_context.Utilisateurs, "Id", "Email", administrateur.Id);
            return View(administrateur);
        }

        // POST: Administrateurs1/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id")] Administrateur administrateur)
        {
            if (id != administrateur.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(administrateur);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AdministrateurExists(administrateur.Id))
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
            ViewData["Id"] = new SelectList(_context.Utilisateurs, "Id", "Email", administrateur.Id);
            return View(administrateur);
        }

        // GET: Administrateurs1/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var administrateur = await _context.Administrateurs
                .Include(a => a.Utilisateur)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (administrateur == null)
            {
                return NotFound();
            }

            return View(administrateur);
        }

        // POST: Administrateurs1/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var administrateur = await _context.Administrateurs.FindAsync(id);
            if (administrateur != null)
            {
                _context.Administrateurs.Remove(administrateur);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AdministrateurExists(int id)
        {
            return _context.Administrateurs.Any(e => e.Id == id);
        }
    }
}
