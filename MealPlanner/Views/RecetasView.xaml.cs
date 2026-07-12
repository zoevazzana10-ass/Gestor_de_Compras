using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using MealPlanner.Data;
using MealPlanner.Models;

namespace MealPlanner.Views
{
    public partial class RecetasView : UserControl
    {
        public RecetasView()
        {
            InitializeComponent();
            Loaded += RecetasView_Loaded;
        }

        private void RecetasView_Loaded(object sender, RoutedEventArgs e)
        {
            CargarComidas();
            CargarComboIngredientes();
        }

        public void CargarComidas()
        {
            using (var context = new AppDbContext())
            {
                var comidas = context.Comidas.ToList();
                var selectedId = (cmbComidas.SelectedItem as Comida)?.Id;

                cmbComidas.ItemsSource = comidas;

                if (selectedId.HasValue)
                {
                    cmbComidas.SelectedItem = comidas.FirstOrDefault(c => c.Id == selectedId.Value);
                }
            }
        }

        public void CargarComboIngredientes()
        {
            using (var context = new AppDbContext())
            {
                cmbIngredientes.ItemsSource = context.Ingredientes.ToList();
            }
        }

        private void cmbComidas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comida = cmbComidas.SelectedItem as Comida;
            if (comida != null)
            {
                CargarReceta(comida.Id);
            }
            else
            {
                dgReceta.ItemsSource = null;
            }
        }

        private void CargarReceta(int comidaId)
        {
            using (var context = new AppDbContext())
            {
                var receta = context.Recetas
                    .Include(r => r.Ingrediente)
                    .Where(r => r.ComidaId == comidaId)
                    .ToList();
                dgReceta.ItemsSource = receta;
            }
        }

        private void btnAgregarIngredienteReceta_Click(object sender, RoutedEventArgs e)
        {
            var comida = cmbComidas.SelectedItem as Comida;
            var ingrediente = cmbIngredientes.SelectedItem as Ingrediente;

            if (comida == null || ingrediente == null)
            {
                MessageBox.Show("Seleccione una comida y un ingrediente.");
                return;
            }

            if (!decimal.TryParse(txtCantidad.Text, out decimal cantidad))
            {
                MessageBox.Show("Ingrese una cantidad válida.");
                return;
            }

            var nuevaReceta = new Receta
            {
                ComidaId = comida.Id,
                IngredienteId = ingrediente.Id,
                Cantidad = cantidad
            };

            using (var context = new AppDbContext())
            {
                context.Recetas.Add(nuevaReceta);
                context.SaveChanges();
            }

            txtCantidad.Clear();
            cmbIngredientes.SelectedItem = null;
            CargarReceta(comida.Id);
        }
    }
}