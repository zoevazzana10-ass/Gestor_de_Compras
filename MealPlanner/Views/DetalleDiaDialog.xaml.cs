using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using MaterialDesignThemes.Wpf;
using MealPlanner.Data;
using MealPlanner.Models;

namespace MealPlanner.Views
{
    public partial class DetalleDiaDialog : UserControl
    {
        private DateTime _fecha;
        public bool NeedsRefresh { get; private set; } = false;

        public DetalleDiaDialog(DateTime fecha)
        {
            InitializeComponent();
            _fecha = fecha;
            txtTitulo.Text = $"Comidas: {fecha.ToShortDateString()}";
            Loaded += DetalleDiaDialog_Loaded;
        }

        private void DetalleDiaDialog_Loaded(object sender, RoutedEventArgs e)
        {
            CargarComidas();
        }

        private void CargarComidas()
        {
            using (var context = new AppDbContext())
            {
                var dict = new Dictionary<string, int> { { "Desayuno", 1 }, { "Almuerzo", 2 }, { "Merienda", 3 }, { "Cena", 4 } };

                var items = context.PlanificacionItems
                    .Include(p => p.Comida)
                    .Where(p => p.Fecha.Date == _fecha.Date)
                    .ToList();

                dgComidasDia.ItemsSource = items
                    .OrderBy(p => dict.ContainsKey(p.TiempoComida) ? dict[p.TiempoComida] : 99)
                    .ToList();
            }
        }

        private void btnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is PlanificacionItem item)
            {
                var result = MessageBox.Show($"¿Desea eliminar '{item.Comida?.Nombre}' del {item.TiempoComida}?", "Confirmar eliminación", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    using (var context = new AppDbContext())
                    {
                        context.PlanificacionItems.Remove(item);
                        context.SaveChanges();
                        NeedsRefresh = true;
                    }
                    CargarComidas();
                }
            }
        }

        private void btnAgregar_Click(object sender, RoutedEventArgs e)
        {
            // Close this dialog but signal that we want to open the Add Dialog
            DialogHost.CloseDialogCommand.Execute("AGREGAR", this);
        }

        private void btnCerrar_Click(object sender, RoutedEventArgs e)
        {
            // Just close normally
            DialogHost.CloseDialogCommand.Execute("CERRAR", this);
        }
    }
}