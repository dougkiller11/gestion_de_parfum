using Gestion_de_parfum.Data;
using Gestion_de_parfum.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Gestion_de_parfum.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ParfumsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ParfumsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Parfum>>> GetParfums()
    {
        return await _context.Parfums
            .Include(p => p.Categorie)
            .AsNoTracking()
            .ToListAsync();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Parfum>> GetParfum(int id)
    {
        var parfum = await _context.Parfums
            .Include(p => p.Categorie)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (parfum == null)
        {
            return NotFound();
        }

        return parfum;
    }

    [HttpPost]
    public async Task<ActionResult<Parfum>> CreateParfum(Parfum parfum)
    {
        _context.Parfums.Add(parfum);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetParfum), new { id = parfum.Id }, parfum);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateParfum(int id, Parfum parfum)
    {
        if (id != parfum.Id)
        {
            return BadRequest();
        }

        _context.Entry(parfum).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException) when (!ParfumExists(id))
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteParfum(int id)
    {
        var parfum = await _context.Parfums.FindAsync(id);
        if (parfum == null)
        {
            return NotFound();
        }

        _context.Parfums.Remove(parfum);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool ParfumExists(int id) =>
        _context.Parfums.Any(e => e.Id == id);
}

