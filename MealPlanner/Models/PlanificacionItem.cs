using System;

namespace MealPlanner.Models
{
    public class PlanificacionItem
    {
        public int Id { get; set; }

        public DateTime Fecha { get; set; }

        // e.g. "Desayuno", "Almuerzo", "Merienda", "Cena"
        public string TiempoComida { get; set; } = string.Empty;

        public int ComidaId { get; set; }
        public Comida? Comida { get; set; }
    }
}
