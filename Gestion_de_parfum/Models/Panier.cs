namespace Gestion_de_parfum.Models;

public class Panier
{
    public int Id { get; set; }
    public double Total { get; set; } = 0;
    public int ClientId { get; set; }
    
    // Navigation property
    public Client Client { get; set; } = null!;
}

