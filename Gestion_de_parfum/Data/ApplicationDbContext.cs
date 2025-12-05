using Microsoft.EntityFrameworkCore;
using Gestion_de_parfum.Models;

namespace Gestion_de_parfum.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Utilisateur> Utilisateurs { get; set; }
    public DbSet<Administrateur> Administrateurs { get; set; }
    public DbSet<Client> Clients { get; set; }
    public DbSet<Categorie> Categories { get; set; }
    public DbSet<Parfum> Parfums { get; set; }
    public DbSet<Panier> Paniers { get; set; }
    public DbSet<Commande> Commandes { get; set; }
    public DbSet<LigneCommande> LigneCommandes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuration de Utilisateur
        modelBuilder.Entity<Utilisateur>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nom).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.Property(e => e.MotDePasse).IsRequired().HasMaxLength(100);
        });

        // Configuration de Administrateur
        modelBuilder.Entity<Administrateur>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Utilisateur)
                .WithOne(u => u.Administrateur)
                .HasForeignKey<Administrateur>(e => e.Id)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configuration de Client
        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Adresse).HasMaxLength(255);
            entity.Property(e => e.Telephone).HasMaxLength(50);
            entity.HasOne(e => e.Utilisateur)
                .WithOne(u => u.Client)
                .HasForeignKey<Client>(e => e.Id)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configuration de Categorie
        modelBuilder.Entity<Categorie>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nom).IsRequired().HasMaxLength(100);
        });

        // Configuration de Parfum
        modelBuilder.Entity<Parfum>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nom).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Marque).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.HasOne(e => e.Categorie)
                .WithMany(c => c.Parfums)
                .HasForeignKey(e => e.CategorieId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configuration de Panier
        modelBuilder.Entity<Panier>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Total).HasDefaultValue(0);
            entity.HasOne(e => e.Client)
                .WithMany(c => c.Paniers)
                .HasForeignKey(e => e.ClientId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configuration de Commande
        modelBuilder.Entity<Commande>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Date).HasDefaultValueSql("datetime('now')");
            entity.Property(e => e.Statut).HasMaxLength(50);
            entity.HasOne(e => e.Client)
                .WithMany(c => c.Commandes)
                .HasForeignKey(e => e.ClientId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configuration de LigneCommande
        modelBuilder.Entity<LigneCommande>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Commande)
                .WithMany(c => c.LigneCommandes)
                .HasForeignKey(e => e.CommandeId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Parfum)
                .WithMany(p => p.LigneCommandes)
                .HasForeignKey(e => e.ParfumId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}


