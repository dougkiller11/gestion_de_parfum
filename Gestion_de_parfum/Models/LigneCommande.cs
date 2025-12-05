namespace Gestion_de_parfum.Models;

public class LigneCommande
{
    public int Id { get; set; }
    public int Quantite { get; set; }
    public double SousTotal { get; set; }
    public int CommandeId { get; set; }
    public int ParfumId { get; set; }
    
    // Navigation properties
    public Commande Commande { get; set; } = null!;
    public Parfum Parfum { get; set; } = null!;
}

