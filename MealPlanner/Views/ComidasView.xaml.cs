using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using MealPlanner.Data;
using MealPlanner.Models;

namespace MealPlanner.Views
{
    public partial class ComidasView : UserControl
    {
        public ComidasView()
        {
            InitializeComponent();
            Loaded += ComidasView_Loaded;
        }

        private void ComidasView_Loaded(object sender, RoutedEventArgs e)
        {
            CargarComidas();
            CargarComboIngredientes();
        }

        private void CargarComidas()
        {
            using (var context = new AppDbContext())
            {
                context.Database.EnsureCreated();
                dgComidas.ItemsSource = context.Comidas.ToList();
            }
        }

        private void CargarComboIngredientes()
        {
            using (var context = new AppDbContext())
            {
                cmbIngredientes.ItemsSource = context.Ingredientes.ToList();
            }
        }

        private void btnAgregarComida_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombreComida.Text))
            {
                MessageBox.Show("El nombre de la comida es obligatorio");
                return;
            }

            var nuevaComida = new Comida
            {
                Nombre = txtNombreComida.Text,
                Categoria = txtCategoriaComida.Text,
                Temperatura = (cmbTemperatura.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Caliente"
            };

            using (var context = new AppDbContext())
            {
                context.Comidas.Add(nuevaComida);
                context.SaveChanges();
            }

            txtNombreComida.Clear();
            txtCategoriaComida.Clear();
            CargarComidas();
        }

        private void dgComidas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comida = dgComidas.SelectedItem as Comida;
            if (comida != null)
            {
                txtTituloReceta.Text = $"Receta: {comida.Nombre}";
                CargarReceta(comida.Id);
            }
            else
            {
                txtTituloReceta.Text = "Receta (Seleccione comida)";
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
            var comida = dgComidas.SelectedItem as Comida;
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
