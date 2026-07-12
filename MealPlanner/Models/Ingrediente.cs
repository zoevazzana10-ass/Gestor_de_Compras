using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MealPlanner.Models
{
    public class Ingrediente
    {
        public int Id { get; set; }

        [Required]
        public string Nombre { get; set; } = string.Empty;

        public string Categoria { get; set; } = string.Empty;

        public string DondeSeConsigue { get; set; } = string.Empty;

        public string UnidadMedida { get; set; } = string.Empty;

        // Navigation property for recipes
        public List<Receta> Recetas { get; set; } = new List<Receta>();
    }
}
