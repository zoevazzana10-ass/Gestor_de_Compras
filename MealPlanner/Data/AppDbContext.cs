using Microsoft.EntityFrameworkCore;
using MealPlanner.Models;

namespace MealPlanner.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext() { }
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Comida> Comidas { get; set; } = null!;
        public DbSet<Ingrediente> Ingredientes { get; set; } = null!;
        public DbSet<Receta> Recetas { get; set; } = null!;
        public DbSet<PlanificacionItem> PlanificacionItems { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("Data Source=mealplanner.sqlite");
            }
        }
    }
}
