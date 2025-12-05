namespace Gestion_de_parfum.Models;


public class Home
{
   
    public string Titre { get; set; } = "Bienvenue dans la gestion de parfums";
   
    public string? Description { get; set; } =
        "Gérez vos parfums, catégories, clients et commandes depuis une seule interface.";
 
    public ICollection<Parfum> ParfumsMisEnAvant { get; set; } = new List<Parfum>();

    public ICollection<Categorie> Categories { get; set; } = new List<Categorie>();
}


