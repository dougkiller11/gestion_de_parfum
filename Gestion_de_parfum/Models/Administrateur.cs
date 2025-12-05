namespace Gestion_de_parfum.Models;

public class Administrateur
{
    public int Id { get; set; }
    
    // Navigation property
    public Utilisateur Utilisateur { get; set; } = null!;
}

