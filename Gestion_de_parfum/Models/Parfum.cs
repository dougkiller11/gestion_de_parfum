namespace Gestion_de_parfum.Models;

using System.ComponentModel.DataAnnotations;

public class Parfum
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Le nom est obligatoire.")]
    [StringLength(100, ErrorMessage = "Le nom doit faire au plus 100 caractères.")]
    public string Nom { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "La marque doit faire au plus 100 caractères.")]
    public string? Marque { get; set; }

    [Range(0.01, 100000, ErrorMessage = "Le prix doit être supérieur à 0.")]
    public double Prix { get; set; }

    [Range(0, 100000, ErrorMessage = "Le stock ne peut pas être négatif.")]
    public int Stock { get; set; }

    [StringLength(500, ErrorMessage = "La description doit faire au plus 500 caractères.")]
    public string? Description { get; set; }

    [StringLength(200)]
    public string? PointsFort1 { get; set; }

    [StringLength(200)]
    public string? PointsFort2 { get; set; }

    [StringLength(200)]
    public string? PointsFort3 { get; set; }

    [StringLength(200)]
    public string? NoteTete { get; set; }

    [StringLength(200)]
    public string? NoteCoeur { get; set; }

    [StringLength(200)]
    public string? NoteFond { get; set; }

    [StringLength(255)]
    [Url(ErrorMessage = "ImageUrl doit être une URL valide.")]
    public string? ImageUrl { get; set; }

    [Required(ErrorMessage = "La catégorie est obligatoire.")]
    public int CategorieId { get; set; }
    
    // Navigation properties
    public Categorie? Categorie { get; set; }
    public ICollection<LigneCommande> LigneCommandes { get; set; } = new List<LigneCommande>();
}


