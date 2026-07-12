namespace MealPlanner.Models
{
    public class Receta
    {
        public int Id { get; set; }

        public int ComidaId { get; set; }
        public Comida? Comida { get; set; }

        public int IngredienteId { get; set; }
        public Ingrediente? Ingrediente { get; set; }

        public decimal Cantidad { get; set; }
    }
}
