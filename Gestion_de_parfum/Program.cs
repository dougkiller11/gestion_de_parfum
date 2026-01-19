using Gestion_de_parfum.Components;
using Microsoft.EntityFrameworkCore;
using Gestion_de_parfum.Data;
using Gestion_de_parfum.Models;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddControllersWithViews();
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add Entity Framework Core
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
Console.WriteLine("==========================================");
Console.WriteLine("🔍 DEBUG: Configuration de la base de données");
Console.WriteLine("==========================================");
Console.WriteLine($"📋 Chaîne de connexion: {connectionString}");
Console.WriteLine("==========================================");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString)
           .EnableSensitiveDataLogging() // Pour voir les valeurs des paramètres
           .EnableDetailedErrors()); // Pour voir les erreurs détaillées

// Auth services
builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/";
    });
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

var app = builder.Build();

// Créer la base de données si elle n'existe pas
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    
    try
    {
        Console.WriteLine("==========================================");
        Console.WriteLine("🔌 DEBUG: Tentative de connexion à SQL Server");
        Console.WriteLine("==========================================");
        
        var context = services.GetRequiredService<ApplicationDbContext>();
        
        Console.WriteLine("📊 Vérification de l'existence de la base de données...");
        var canConnect = context.Database.CanConnect();
        Console.WriteLine($"✅ CanConnect: {canConnect}");
        
        if (!canConnect)
        {
            Console.WriteLine("⚠️  La base de données n'existe pas ou n'est pas accessible.");
            Console.WriteLine("🔨 Tentative de création de la base de données...");
        }
        else
        {
            Console.WriteLine("✅ La base de données existe et est accessible.");
        }
        
        Console.WriteLine("📦 Création des tables si nécessaire...");
        context.Database.EnsureCreated();

        // Seed de quelques catégories et parfums si la base est vide
        if (!context.Categories.Any())
        {
            var catHomme = new Categorie { Nom = "Homme" };
            var catFemme = new Categorie { Nom = "Femme" };
            var catMixte = new Categorie { Nom = "Mixte" };
            context.Categories.AddRange(catHomme, catFemme, catMixte);
            context.SaveChanges();
        }

        if (!context.Parfums.Any())
        {
            var categories = context.Categories.ToList();
            var catHommeId = categories.FirstOrDefault(c => c.Nom == "Homme")?.Id ?? 0;
            var catFemmeId = categories.FirstOrDefault(c => c.Nom == "Femme")?.Id ?? 0;
            var catMixteId = categories.FirstOrDefault(c => c.Nom == "Mixte")?.Id ?? 0;

            context.Parfums.AddRange(
                new Parfum
                {
                    Nom = "Mont Émeraude",
                    Marque = "Essence Luxe",
                    Description = "Notes boisées, mousse de chêne et agrumes verts.",
                    Prix = 89.90,
                    Stock = 15,
                    CategorieId = catHommeId,
                    ImageUrl = "https://images.unsplash.com/photo-1541643600914-78b084683601?auto=format&fit=crop&w=900&q=80"
                },
                new Parfum
                {
                    Nom = "Jardin d'Or",
                    Marque = "Essence Luxe",
                    Description = "Fleurs blanches, miel léger et vanille dorée.",
                    Prix = 94.50,
                    Stock = 12,
                    CategorieId = catFemmeId,
                    ImageUrl = "https://images.unsplash.com/photo-1524592094714-0f0654e20314?auto=format&fit=crop&w=900&q=80"
                },
                new Parfum
                {
                    Nom = "Brume Nocturne",
                    Marque = "Essence Luxe",
                    Description = "Encens, poivre rose et santal crémeux.",
                    Prix = 102.00,
                    Stock = 8,
                    CategorieId = catMixteId,
                    ImageUrl = "https://images.unsplash.com/photo-1506617420156-8e4536971650?auto=format&fit=crop&w=900&q=80"
                },
                new Parfum
                {
                    Nom = "Lueur Boréale",
                    Marque = "Essence Luxe",
                    Description = "Notes fraîches de pin, genièvre et ambre clair.",
                    Prix = 88.00,
                    Stock = 10,
                    CategorieId = catHommeId,
                    ImageUrl = "https://images.unsplash.com/photo-1445205170230-053b83016050?auto=format&fit=crop&w=900&q=80"
                },
                new Parfum
                {
                    Nom = "Rosée Solaire",
                    Marque = "Essence Luxe",
                    Description = "Pivoine, bergamote et musc doux.",
                    Prix = 92.00,
                    Stock = 20,
                    CategorieId = catFemmeId,
                    ImageUrl = "https://images.unsplash.com/photo-1487412720507-e7ab37603c6f?auto=format&fit=crop&w=900&q=80"
                },
                new Parfum
                {
                    Nom = "Éclat Minéral",
                    Marque = "Essence Luxe",
                    Description = "Minéral salé, vétiver clair et accord propre.",
                    Prix = 96.00,
                    Stock = 18,
                    CategorieId = catMixteId,
                    ImageUrl = "https://images.unsplash.com/photo-1556228578-0e066e17d25c?auto=format&fit=crop&w=900&q=80"
                }
            );
            context.SaveChanges();
        }

        // Seed admin user if missing
        var adminEmail = "admin@admin.com";
        var admin = context.Utilisateurs.FirstOrDefault(u => u.Email == adminEmail);
        if (admin == null)
        {
            admin = new Utilisateur
            {
                Nom = "Admin",
                Email = adminEmail,
                MotDePasse = "admin"
            };
            context.Utilisateurs.Add(admin);
            context.SaveChanges();
        }
        var adminExists = context.Administrateurs.Any(a => a.Id == admin.Id);
        if (!adminExists)
        {
            context.Administrateurs.Add(new Administrateur { Id = admin.Id });
            context.SaveChanges();
        }
        Console.WriteLine("✅ Base de données et tables créées avec succès!");
        
        // Vérifier que la connexion fonctionne
        var testQuery = context.Database.ExecuteSqlRaw("SELECT 1");
        Console.WriteLine($"📋 Test de requête SQL: OK");
        
        Console.WriteLine("==========================================");
        Console.WriteLine("✅ Connexion à la base de données réussie!");
        Console.WriteLine("==========================================");
    }
    catch (Exception ex)
    {
        Console.WriteLine("==========================================");
        Console.WriteLine("❌ ERREUR: Problème de connexion à la base de données");
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
        
        logger.LogError(ex, "Une erreur est survenue lors de la création de la base de données.");
        
        // Ne pas arrêter l'application, continuer quand même
        Console.WriteLine("⚠️  L'application continue malgré l'erreur de base de données.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.UseSession();

// Routes MVC - Doit être avant MapRazorComponents
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Routes spécifiques
app.MapControllerRoute(
    name: "account",
    pattern: "Account/{action=Login}/{id?}",
    defaults: new { controller = "Account" });

app.MapControllerRoute(
    name: "boutique",
    pattern: "Boutique/{action=Index}/{id?}",
    defaults: new { controller = "Boutique" });

app.MapControllerRoute(
    name: "admin-products",
    pattern: "AdminProducts/{action=Dashboard}/{id?}",
    defaults: new { controller = "AdminProducts" });

// Page d'accueil accessible à tous
app.MapGet("/", () => Results.Redirect("/Home/Index"));

// Route pour tous les autres contrôleurs
app.MapControllers();

// Razor Components (doit être en dernier)
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .RequireAuthorization();

app.Run();
