namespace Gestion_de_parfum.Models;

public class Commande
{
    public int Id { get; set; }
    public DateTime Date { get; set; } = DateTime.Now;
    public double Total { get; set; }
    public string? Statut { get; set; }
    public int ClientId { get; set; }
    
    // Navigation properties
    public Client Client { get; set; } = null!;
    public ICollection<LigneCommande> LigneCommandes { get; set; } = new List<LigneCommande>();
}

