using System.Security.Claims;
using Gestion_de_parfum.Data;
using Gestion_de_parfum.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Gestion_de_parfum.Controllers;

public class AccountController : Controller
{
    private readonly ApplicationDbContext _context;

    public AccountController(ApplicationDbContext context)
    {
        _context = context;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        Console.WriteLine("==========================================");
        Console.WriteLine("🔐 DEBUG: Début du processus de connexion");
        Console.WriteLine("==========================================");
        Console.WriteLine($"📋 Méthode HTTP: {Request.Method}");
        Console.WriteLine($"📋 Content-Type: {Request.ContentType}");
        Console.WriteLine($"📋 Données du formulaire reçues:");
        Console.WriteLine($"   - Email: '{model.Email}' (longueur: {model.Email?.Length ?? 0})");
        Console.WriteLine($"   - Password: [masqué] (longueur: {model.Password?.Length ?? 0})");
        Console.WriteLine($"   - RememberMe: {model.RememberMe}");
        Console.WriteLine($"   - ReturnUrl: {model.ReturnUrl}");
        
        // Vérifier les données brutes de la requête
        Console.WriteLine($"📋 Données brutes de la requête:");
        if (Request.HasFormContentType)
        {
            foreach (var key in Request.Form.Keys)
            {
                Console.WriteLine($"   - {key}: {Request.Form[key]}");
            }
        }
        
        if (!ModelState.IsValid)
        {
            Console.WriteLine("❌ ERREUR: ModelState invalide");
            Console.WriteLine($"   Nombre d'erreurs: {ModelState.ErrorCount}");
            foreach (var error in ModelState)
            {
                Console.WriteLine($"   - {error.Key}: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
            }
            return View(model);
        }
        
        Console.WriteLine("✅ Validation du modèle réussie");
        Console.WriteLine($"🔍 Recherche de l'utilisateur avec l'email: {model.Email}");

        var user = await _context.Utilisateurs
            .Include(u => u.Administrateur)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == model.Email && u.MotDePasse == model.Password);

        if (user == null)
        {
            Console.WriteLine("❌ ERREUR: Identifiants invalides");
            ModelState.AddModelError(string.Empty, "Identifiants invalides.");
            return View(model);
        }
        
        Console.WriteLine($"✅ Utilisateur trouvé: {user.Nom} (ID: {user.Id})");

        var isAdmin = user.Administrateur != null;
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Nom),
            new(ClaimTypes.Email, user.Email)
        };
        if (isAdmin)
        {
            claims.Add(new Claim(ClaimTypes.Role, "Admin"));
        }

        Console.WriteLine("🔐 Configuration de l'authentification...");
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        Console.WriteLine("🔐 Connexion de l'utilisateur...");
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(30) : DateTimeOffset.UtcNow.AddHours(8)
            });
        Console.WriteLine("✅ Utilisateur connecté avec succès");

        Console.WriteLine("🔄 Redirection...");
        if (isAdmin)
        {
            Console.WriteLine("   Redirection vers: /AdminProducts/Dashboard (admin)");
            return RedirectToAction("Dashboard", "AdminProducts");
        }

        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
        {
            Console.WriteLine($"   Redirection vers: {model.ReturnUrl}");
            return Redirect(model.ReturnUrl);
        }

        Console.WriteLine("   Redirection vers: /Boutique");
        Console.WriteLine("==========================================");
        Console.WriteLine("✅ Connexion réussie!");
        Console.WriteLine("==========================================");
        
        return RedirectToAction("Index", "Boutique");
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Register(string? returnUrl = null)
    {
        return View(new RegisterViewModel { ReturnUrl = returnUrl });
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        Console.WriteLine("==========================================");
        Console.WriteLine("📝 DEBUG: Début du processus d'inscription");
        Console.WriteLine("==========================================");
        Console.WriteLine($"📋 Méthode HTTP: {Request.Method}");
        Console.WriteLine($"📋 Content-Type: {Request.ContentType}");
        Console.WriteLine($"📋 Données du formulaire reçues:");
        Console.WriteLine($"   - Nom: '{model.Nom}' (longueur: {model.Nom?.Length ?? 0})");
        Console.WriteLine($"   - Email: '{model.Email}' (longueur: {model.Email?.Length ?? 0})");
        Console.WriteLine($"   - Password: [masqué] (longueur: {model.Password?.Length ?? 0})");
        Console.WriteLine($"   - ConfirmPassword: [masqué] (longueur: {model.ConfirmPassword?.Length ?? 0})");
        Console.WriteLine($"   - ReturnUrl: {model.ReturnUrl}");
        
        // Vérifier les données brutes de la requête
        Console.WriteLine($"📋 Données brutes de la requête:");
        if (Request.HasFormContentType)
        {
            foreach (var key in Request.Form.Keys)
            {
                Console.WriteLine($"   - {key}: {Request.Form[key]}");
            }
        }
        
        if (!ModelState.IsValid)
        {
            Console.WriteLine("❌ ERREUR: ModelState invalide");
            Console.WriteLine($"   Nombre d'erreurs: {ModelState.ErrorCount}");
            foreach (var error in ModelState)
            {
                Console.WriteLine($"   - {error.Key}: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
            }
            return View(model);
        }
        
        Console.WriteLine("✅ Validation du modèle réussie");

        // Vérifier si l'email existe déjà
        Console.WriteLine($"🔍 Vérification si l'email existe déjà: {model.Email}");
        var exists = await _context.Utilisateurs.AnyAsync(u => u.Email == model.Email);
        Console.WriteLine($"   Email existe: {exists}");
        
        if (exists)
        {
            Console.WriteLine("❌ ERREUR: Email déjà utilisé");
            ModelState.AddModelError(nameof(model.Email), "Un utilisateur avec cet email existe déjà.");
            return View(model);
        }

        try
        {
            Console.WriteLine("🔨 Création de l'objet Utilisateur...");
            // Créer l'utilisateur
            var user = new Utilisateur
            {
                Nom = model.Nom,
                Email = model.Email,
                // Pour l'instant, mot de passe en clair pour rester compatible avec le login existant.
                // Plus tard, il faudra le hasher et adapter le login.
                MotDePasse = model.Password
            };
            Console.WriteLine($"   Utilisateur créé - Nom: {user.Nom}, Email: {user.Email}");

            Console.WriteLine("💾 Ajout de l'utilisateur au contexte...");
            _context.Utilisateurs.Add(user);
            Console.WriteLine($"   État de l'entité: {_context.Entry(user).State}");
            
            Console.WriteLine("💾 Sauvegarde dans la base de données...");
            var savedCount = await _context.SaveChangesAsync();
            Console.WriteLine($"✅ {savedCount} entité(s) sauvegardée(s)");
            Console.WriteLine($"   ID de l'utilisateur créé: {user.Id}");

            // Créer un Client associé pour permettre les commandes et paniers
            // Note: Le Client utilise le même ID que l'Utilisateur (relation 1-1)
            try
            {
                Console.WriteLine("🔨 Création du Client associé...");
                var client = new Client
                {
                    Id = user.Id,
                    Adresse = null,
                    Telephone = null
                };
                Console.WriteLine($"   Client créé avec ID: {client.Id}");

                Console.WriteLine("💾 Ajout du Client au contexte...");
                _context.Clients.Add(client);
                Console.WriteLine($"   État de l'entité: {_context.Entry(client).State}");
                
                Console.WriteLine("💾 Sauvegarde du Client dans la base de données...");
                var clientSavedCount = await _context.SaveChangesAsync();
                Console.WriteLine($"✅ {clientSavedCount} entité(s) sauvegardée(s) pour le Client");
            }
            catch (Exception clientEx)
            {
                Console.WriteLine("⚠️  ERREUR lors de la création du Client:");
                Console.WriteLine($"   Type: {clientEx.GetType().Name}");
                Console.WriteLine($"   Message: {clientEx.Message}");
                Console.WriteLine($"   Stack Trace: {clientEx.StackTrace}");
                // Si la création du Client échoue, on continue quand même
                // L'utilisateur peut toujours se connecter
            }

            Console.WriteLine("🔐 Configuration de l'authentification...");
            // Connexion automatique après inscription
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.Nom),
                new(ClaimTypes.Email, user.Email)
            };
            Console.WriteLine($"   Claims créés: {claims.Count}");

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            Console.WriteLine("🔐 Connexion de l'utilisateur...");
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                });
            Console.WriteLine("✅ Utilisateur connecté avec succès");

            Console.WriteLine("🔄 Redirection...");
            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                Console.WriteLine($"   Redirection vers: {model.ReturnUrl}");
                return Redirect(model.ReturnUrl);
            }

            Console.WriteLine("   Redirection vers: /Boutique");
            Console.WriteLine("==========================================");
            Console.WriteLine("✅ Inscription réussie!");
            Console.WriteLine("==========================================");
            
            return RedirectToAction("Index", "Boutique");
        }
        catch (Exception ex)
        {
            Console.WriteLine("==========================================");
            Console.WriteLine("❌ ERREUR: Exception lors de l'inscription");
            Console.WriteLine("==========================================");
            Console.WriteLine($"🔴 Type d'erreur: {ex.GetType().Name}");
            Console.WriteLine($"🔴 Message: {ex.Message}");
            Console.WriteLine($"🔴 Source: {ex.Source}");
            
            if (ex.InnerException != null)
            {
                Console.WriteLine($"🔴 Exception interne: {ex.InnerException.GetType().Name}");
                Console.WriteLine($"🔴 Message interne: {ex.InnerException.Message}");
            }
            
            Console.WriteLine("🔴 Stack Trace:");
            Console.WriteLine(ex.StackTrace);
            Console.WriteLine("==========================================");
            
            // En cas d'erreur, afficher un message et retourner à la vue
            ModelState.AddModelError(string.Empty, $"Une erreur est survenue lors de l'inscription : {ex.Message}");
            // Log l'erreur si nécessaire
            return View(model);
        }
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }
}


