using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using MealPlanner.Data;

namespace MealPlanner.Views
{
    public class IngredienteCompraItem
    {
        public string Nombre { get; set; } = string.Empty;
        public decimal CantidadTotal { get; set; }
        public string UnidadMedida { get; set; } = string.Empty;
        public string DondeSeConsigue { get; set; } = string.Empty;
    }

    public partial class ListaComprasView : UserControl
    {
        public ListaComprasView()
        {
            InitializeComponent();
            dpDesde.SelectedDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            dpHasta.SelectedDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month));
        }

        private void btnGenerar_Click(object sender, RoutedEventArgs e)
        {
            if (dpDesde.SelectedDate == null || dpHasta.SelectedDate == null)
            {
                MessageBox.Show("Seleccione un rango de fechas válido.");
                return;
            }

            var fechaInicio = dpDesde.SelectedDate.Value;
            var fechaFin = dpHasta.SelectedDate.Value;

            using (var context = new AppDbContext())
            {
                context.Database.EnsureCreated();

                // Get all planned meals in the date range
                var planes = context.PlanificacionItems
                    .Where(p => p.Fecha >= fechaInicio && p.Fecha <= fechaFin)
                    .Select(p => p.ComidaId)
                    .ToList();

                // Get all recipes for those meals, including the ingredient details
                var recetas = context.Recetas
                    .Include(r => r.Ingrediente)
                    .Where(r => planes.Contains(r.ComidaId))
                    .ToList();

                // Aggregate by Ingredient ID to get totals
                var listaAgrupada = recetas
                    .GroupBy(r => r.IngredienteId)
                    .Select(g => new IngredienteCompraItem
                    {
                        Nombre = g.First().Ingrediente!.Nombre,
                        CantidadTotal = g.Sum(r => r.Cantidad * planes.Count(id => id == r.ComidaId)),
                        UnidadMedida = g.First().Ingrediente!.UnidadMedida,
                        DondeSeConsigue = g.First().Ingrediente!.DondeSeConsigue
                    })
                    .OrderBy(i => i.DondeSeConsigue)
                    .ThenBy(i => i.Nombre)
                    .ToList();

                dgListaCompras.ItemsSource = listaAgrupada;
            }
        }
    }
}
