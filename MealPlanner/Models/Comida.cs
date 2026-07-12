using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MealPlanner.Models
{
    public class Comida
    {
        public int Id { get; set; }

        [Required]
        public string Nombre { get; set; } = string.Empty;

        public string Categoria { get; set; } = string.Empty;

        public string Temperatura { get; set; } = "Caliente"; // "Frio" o "Caliente"

        // Navigation property for recipes
        public List<Receta> Recetas { get; set; } = new List<Receta>();
    }
}
