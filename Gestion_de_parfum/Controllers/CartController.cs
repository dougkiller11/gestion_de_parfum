using System.Security.Claims;
using System.Text.Json;
using Gestion_de_parfum.Data;
using Gestion_de_parfum.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Gestion_de_parfum.Controllers;

public class CartController : Controller
{
    private readonly ApplicationDbContext _context;
    private const string CartSessionKey = "cart";

    public CartController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Add a product to cart (POST) and fallback GET if POST est bloqué
    [HttpPost]
    [IgnoreAntiforgeryToken] // évite un 400 si le cookie/token est bloqué côté client
    public Task<IActionResult> Add(int id, int qty = 1, string? returnUrl = null) => AddInternal(id, qty, returnUrl);

    // Fallback GET (permet d'ajouter même si le POST est filtré par un adblock/proxy)
    [HttpGet]
    public Task<IActionResult> AddQuick(int id, int qty = 1, string? returnUrl = null) => AddInternal(id, qty, returnUrl);

    private async Task<IActionResult> AddInternal(int id, int qty, string? returnUrl)
    {
        if (qty < 1) qty = 1;

        var parfum = await _context.Parfums.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        if (parfum == null)
        {
            return NotFound();
        }

        var cart = GetCart();
        var existing = cart.FirstOrDefault(c => c.ParfumId == id);
        if (existing == null)
        {
            cart.Add(new CartItem
            {
                ParfumId = id,
                Nom = parfum.Nom,
                Prix = parfum.Prix,
                Quantite = qty,
                Taille = "100ml"
            });
        }
        else
        {
            existing.Quantite += qty;
        }

        SaveCart(cart);

        return Redirect(returnUrl ?? Url.Action("Index", "Boutique")!);
    }

    // Simple cart page
    [HttpGet]
    public IActionResult Index()
    {
        var cart = GetCart();
        ViewBag.Total = cart.Sum(c => ComputeUnitPrice(c) * c.Quantite);
        return View(cart);
    }

    // Update qty/size in cart
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public IActionResult Update(int id, int qty = 1, string? taille = null)
    {
        if (qty < 1) qty = 1;
        var cart = GetCart();
        var item = cart.FirstOrDefault(c => c.ParfumId == id);
        if (item != null)
        {
            item.Quantite = qty;
            if (!string.IsNullOrWhiteSpace(taille))
                item.Taille = taille;
            SaveCart(cart);
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult Clear()
    {
        SaveCart(new List<CartItem>());
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Payment()
    {
        var cart = GetCart();
        ViewBag.Total = cart.Sum(c => ComputeUnitPrice(c) * c.Quantite);
        return View(cart);
    }

    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Confirmation(string addr1, string addr2, string postal, string country,
                                      string cardName, string cardNumber, string expiry, string cvv)
    {
        var cart = GetCart();
        if (cart == null || cart.Count == 0)
        {
            return RedirectToAction(nameof(Index));
        }

        // Auth check: we need the user ID to lier la commande
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr))
        {
            return RedirectToAction("Login", "Account", new { returnUrl = Url.Action(nameof(Payment), "Cart") });
        }
        var userId = int.Parse(userIdStr);

        // S'assurer qu'un client existe pour cet utilisateur (sinon FK Commande -> Client échoue)
        var client = await _context.Clients.FindAsync(userId);
        if (client == null)
        {
            _context.Clients.Add(new Client
            {
                Id = userId,
                Adresse = addr1 ?? string.Empty,
                Telephone = string.Empty
            });
            await _context.SaveChangesAsync();
        }

        var total = cart.Sum(c => ComputeUnitPrice(c) * c.Quantite);
        // Créer la commande en base
        var commande = new Gestion_de_parfum.Models.Commande
        {
            ClientId = userId,
            Date = DateTime.Now,
            Total = total,
            Statut = "En attente de livraison"
        };
        _context.Commandes.Add(commande);
        await _context.SaveChangesAsync(); // pour récupérer l'Id

        // Ajouter les lignes
        foreach (var item in cart)
        {
            _context.LigneCommandes.Add(new Gestion_de_parfum.Models.LigneCommande
            {
                CommandeId = commande.Id,
                ParfumId = item.ParfumId,
                Quantite = item.Quantite,
                SousTotal = ComputeUnitPrice(item) * item.Quantite
            });
        }
        await _context.SaveChangesAsync();

        var orderId = $"CMD-{commande.Id:000000}";
        var cardDigits = (cardNumber ?? string.Empty).Replace(" ", "");
        var first4 = cardDigits.Length >= 4 ? cardDigits[..4] : cardDigits;
        var last4 = new string(cardDigits.TakeLast(Math.Min(4, cardDigits.Length)).ToArray());
        var masked = $"{first4.PadRight(4, '*')} **** **** {last4.PadLeft(4, '*')}";

        var summary = new CheckoutSummary
        {
            OrderId = orderId,
            Total = total,
            CardName = cardName ?? "",
            CardMasked = masked,
            CardFirst4 = first4,
            CardLast4 = last4,
            OrderDbId = commande.Id,
            Items = cart.ToList()
        };

        // vider le panier après confirmation
        SaveCart(new List<CartItem>());

        return View("Confirmation", summary);
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> History()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login", "Account");
        var userId = int.Parse(userIdStr);

        var commandes = await _context.Commandes
            .Include(c => c.LigneCommandes)
                .ThenInclude(l => l.Parfum)
            .Where(c => c.ClientId == userId)
            .OrderByDescending(c => c.Date)
            .ToListAsync();

        return View("History", commandes);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> AdminHistory()
    {
        var commandes = await _context.Commandes
            .Include(c => c.Client)
                .ThenInclude(cl => cl.Utilisateur)
            .Include(c => c.LigneCommandes)
                .ThenInclude(l => l.Parfum)
            .OrderByDescending(c => c.Date)
            .ToListAsync();

        return View("AdminHistory", commandes);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, string statut, string? returnUrl = null)
    {
        var commande = await _context.Commandes.FirstOrDefaultAsync(c => c.Id == id);
        if (commande == null) return NotFound();
        commande.Statut = statut;
        await _context.SaveChangesAsync();
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);
        return RedirectToAction(nameof(AdminHistory));
    }

    private List<CartItem> GetCart()
    {
        var json = HttpContext.Session.GetString(CartSessionKey);
        if (string.IsNullOrEmpty(json)) return new List<CartItem>();
        return JsonSerializer.Deserialize<List<CartItem>>(json) ?? new List<CartItem>();
    }

    private void SaveCart(List<CartItem> items)
    {
        var json = JsonSerializer.Serialize(items);
        HttpContext.Session.SetString(CartSessionKey, json);
    }

    private static double ComputeUnitPrice(CartItem item)
    {
        var basePrice = item.Prix;
        var taille = (item.Taille ?? string.Empty).Trim().ToLowerInvariant();
        var mult = taille switch
        {
            "75ml" => 0.8,
            "200ml" => 2.0,
            _ => 1.0
        };
        return basePrice * mult;
    }
}

public class CartItem
{
    public int ParfumId { get; set; }
    public string Nom { get; set; } = string.Empty;
    public double Prix { get; set; }
    public int Quantite { get; set; }
    public string Taille { get; set; } = "100ml";
}

public class CheckoutSummary
{
    public string OrderId { get; set; } = string.Empty;
    public int OrderDbId { get; set; }
    public double Total { get; set; }
    public string CardName { get; set; } = string.Empty;
    public string CardMasked { get; set; } = string.Empty;
    public string CardFirst4 { get; set; } = string.Empty;
    public string CardLast4 { get; set; } = string.Empty;
    public List<CartItem> Items { get; set; } = new();
}

