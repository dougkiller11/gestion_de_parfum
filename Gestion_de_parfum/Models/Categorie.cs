namespace Gestion_de_parfum.Models;

public class Categorie
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    
    // Navigation property
    public ICollection<Parfum> Parfums { get; set; } = new List<Parfum>();
}

