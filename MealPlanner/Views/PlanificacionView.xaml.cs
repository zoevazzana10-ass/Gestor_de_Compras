using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using MealPlanner.Data;
using MealPlanner.Models;

namespace MealPlanner.Views
{
    public partial class PlanificacionView : UserControl
    {
        public PlanificacionView()
        {
            InitializeComponent();
            Loaded += PlanificacionView_Loaded;
            dpFecha.SelectedDate = DateTime.Today;
        }

        private void PlanificacionView_Loaded(object sender, RoutedEventArgs e)
        {
            CargarComidas();
            CargarPlanificacion();
        }

        private void CargarComidas()
        {
            using (var context = new AppDbContext())
            {
                context.Database.EnsureCreated();
                cmbComidas.ItemsSource = context.Comidas.ToList();
            }
        }

        private void CargarPlanificacion()
        {
            using (var context = new AppDbContext())
            {
                var dict = new Dictionary<string, int> { { "Desayuno", 1 }, { "Almuerzo", 2 }, { "Merienda", 3 }, { "Cena", 4 } };

                var items = context.PlanificacionItems
                    .Include(p => p.Comida)
                    .ToList();

                dgPlanificacion.ItemsSource = items
                    .OrderBy(p => p.Fecha)
                    .ThenBy(p => dict.ContainsKey(p.TiempoComida) ? dict[p.TiempoComida] : 99)
                    .ToList();
            }
        }

        private void btnAgregarPlan_Click(object sender, RoutedEventArgs e)
        {
            var fecha = dpFecha.SelectedDate;
            var tiempo = (cmbTiempo.SelectedItem as ComboBoxItem)?.Content?.ToString();
            var comida = cmbComidas.SelectedItem as Comida;

            if (fecha == null || string.IsNullOrEmpty(tiempo) || comida == null)
            {
                MessageBox.Show("Por favor seleccione Fecha, Tiempo de Comida y Comida.");
                return;
            }

            var planItem = new PlanificacionItem
            {
                Fecha = fecha.Value,
                TiempoComida = tiempo,
                ComidaId = comida.Id
            };

            using (var context = new AppDbContext())
            {
                context.PlanificacionItems.Add(planItem);
                context.SaveChanges();
            }

            cmbComidas.SelectedItem = null;
            CargarPlanificacion();
        }
    }
}
