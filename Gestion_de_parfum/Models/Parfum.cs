namespace Gestion_de_parfum.Models;

public class Parfum
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string? Marque { get; set; }
    public double Prix { get; set; }
    public int Stock { get; set; }
    public string? Description { get; set; }
    public int? CategorieId { get; set; }
    
    // Navigation properties
    public Categorie? Categorie { get; set; }
    public ICollection<LigneCommande> LigneCommandes { get; set; } = new List<LigneCommande>();
}


