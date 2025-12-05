namespace Gestion_de_parfum.Models;

public class Client
{
    public int Id { get; set; }
    public string? Adresse { get; set; }
    public string? Telephone { get; set; }
    
    // Navigation properties
    public Utilisateur Utilisateur { get; set; } = null!;
    public ICollection<Panier> Paniers { get; set; } = new List<Panier>();
    public ICollection<Commande> Commandes { get; set; } = new List<Commande>();
}

