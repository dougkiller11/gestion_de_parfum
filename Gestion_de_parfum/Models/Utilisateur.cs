namespace Gestion_de_parfum.Models;

public class Utilisateur
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string MotDePasse { get; set; } = string.Empty;
    
    // Navigation properties
    public Administrateur? Administrateur { get; set; }
    public Client? Client { get; set; }
}

