using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Xunit;
using MealPlanner.Data;
using MealPlanner.Models;

namespace MealPlanner.Tests
{
    public class ShoppingListTests
    {
        [Fact]
        public void TestShoppingListCalculation()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite("DataSource=:memory:")
                .Options;

            using (var context = new AppDbContext(options))
            {
                context.Database.OpenConnection();
                context.Database.EnsureCreated();

                // 1. Add Ingredients
                var pan = new Ingrediente { Nombre = "Pan", Categoria = "Panadería", DondeSeConsigue = "Super", UnidadMedida = "Unidad" };
                var queso = new Ingrediente { Nombre = "Queso", Categoria = "Lácteos", DondeSeConsigue = "Super", UnidadMedida = "Gramos" };
                context.Ingredientes.AddRange(pan, queso);
                context.SaveChanges();

                // 2. Add Meal
                var tostado = new Comida { Nombre = "Tostado", Categoria = "Desayuno", Temperatura = "Caliente" };
                context.Comidas.Add(tostado);
                context.SaveChanges();

                // 3. Add Recipe
                context.Recetas.Add(new Receta { ComidaId = tostado.Id, IngredienteId = pan.Id, Cantidad = 2 });
                context.Recetas.Add(new Receta { ComidaId = tostado.Id, IngredienteId = queso.Id, Cantidad = 50 });
                context.SaveChanges();

                // 4. Plan the meal twice
                var today = DateTime.Today;
                context.PlanificacionItems.Add(new PlanificacionItem { Fecha = today, TiempoComida = "Desayuno", ComidaId = tostado.Id });
                context.PlanificacionItems.Add(new PlanificacionItem { Fecha = today.AddDays(1), TiempoComida = "Desayuno", ComidaId = tostado.Id });
                context.SaveChanges();

                // 5. Calculate (mocking the view logic)
                var planes = context.PlanificacionItems.Select(p => p.ComidaId).ToList();
                var recetas = context.Recetas.Where(r => planes.Contains(r.ComidaId)).ToList();

                var lista = recetas.GroupBy(r => r.IngredienteId)
                    .Select(g => new
                    {
                        IngredienteId = g.Key,
                        CantidadTotal = g.Sum(r => r.Cantidad * planes.Count(id => id == r.ComidaId))
                    }).ToList();

                // 6. Assert
                Assert.Equal(2, lista.Count); // Pan and Queso

                var panResult = lista.First(x => x.IngredienteId == pan.Id);
                Assert.Equal(4, panResult.CantidadTotal); // 2 units * 2 meals = 4

                var quesoResult = lista.First(x => x.IngredienteId == queso.Id);
                Assert.Equal(100, quesoResult.CantidadTotal); // 50 units * 2 meals = 100
            }
        }
    }
}
