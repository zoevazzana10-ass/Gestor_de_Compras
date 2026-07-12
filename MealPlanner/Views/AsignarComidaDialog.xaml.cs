using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using MaterialDesignThemes.Wpf;
using MealPlanner.Data;
using MealPlanner.Models;

namespace MealPlanner.Views
{
    public partial class AsignarComidaDialog : UserControl
    {
        private DateTime _fecha;
        private List<Comida> _todasLasComidas = new();

        public AsignarComidaDialog(DateTime fecha)
        {
            InitializeComponent();
            _fecha = fecha;
            txtTitulo.Text = $"Asignar para {fecha.ToShortDateString()}";
            Loaded += AsignarComidaDialog_Loaded;
        }

        private void AsignarComidaDialog_Loaded(object sender, RoutedEventArgs e)
        {
            using (var context = new AppDbContext())
            {
                _todasLasComidas = context.Comidas.ToList();

                var categorias = _todasLasComidas
                    .Select(c => c.Categoria)
                    .Distinct()
                    .Where(c => !string.IsNullOrEmpty(c))
                    .ToList();

                categorias.Insert(0, "Todas");
                cmbFiltroCategoria.ItemsSource = categorias;
                cmbFiltroCategoria.SelectedIndex = 0;
                cmbFiltroTemperatura.SelectedIndex = 0;
            }

            AplicarFiltros();
        }

        private void Filtro_Changed(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded) return;
            AplicarFiltros();
        }

        private void AplicarFiltros()
        {
            var filtradas = _todasLasComidas.AsEnumerable();

            string busqueda = txtBuscar.Text.Trim().ToLower();
            if (!string.IsNullOrEmpty(busqueda))
            {
                filtradas = filtradas.Where(c => c.Nombre.ToLower().Contains(busqueda));
            }

            string? catFiltro = cmbFiltroCategoria.SelectedItem as string;
            if (catFiltro != null && catFiltro != "Todas")
            {
                filtradas = filtradas.Where(c => c.Categoria == catFiltro);
            }

            string? tempFiltro = (cmbFiltroTemperatura.SelectedItem as ComboBoxItem)?.Content?.ToString();
            if (tempFiltro != null && tempFiltro != "Todas")
            {
                filtradas = filtradas.Where(c => c.Temperatura == tempFiltro);
            }

            lbComidas.ItemsSource = filtradas.ToList();
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            var comidaSeleccionada = lbComidas.SelectedItem as Comida;
            var tiempoComida = (cmbTiempoComida.SelectedItem as ComboBoxItem)?.Content?.ToString();

            if (comidaSeleccionada == null)
            {
                MessageBox.Show("Por favor seleccione una comida de la lista.");
                return;
            }

            if (string.IsNullOrEmpty(tiempoComida))
            {
                MessageBox.Show("Por favor seleccione un tiempo de comida.");
                return;
            }

            var planItem = new PlanificacionItem
            {
                Fecha = _fecha,
                TiempoComida = tiempoComida,
                ComidaId = comidaSeleccionada.Id
            };

            using (var context = new AppDbContext())
            {
                context.PlanificacionItems.Add(planItem);
                context.SaveChanges();
            }

            DialogHost.CloseDialogCommand.Execute(true, this);
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogHost.CloseDialogCommand.Execute(false, this);
        }
    }
}