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
            CargarCategorias();
        }

        private void CargarComidas()
        {
            using (var context = new AppDbContext())
            {
                context.Database.EnsureCreated();
                dgComidas.ItemsSource = context.Comidas.ToList();
            }
        }

        private void CargarCategorias()
        {
            using (var context = new AppDbContext())
            {
                var categorias = context.Comidas
                    .Select(c => c.Categoria)
                    .Distinct()
                    .Where(c => !string.IsNullOrEmpty(c))
                    .ToList();
                cmbCategoriaComida.ItemsSource = categorias;
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
                Categoria = cmbCategoriaComida.Text,
                Temperatura = (cmbTemperatura.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Caliente"
            };

            using (var context = new AppDbContext())
            {
                context.Comidas.Add(nuevaComida);
                context.SaveChanges();
            }

            txtNombreComida.Clear();
            cmbCategoriaComida.Text = "";
            CargarComidas();
            CargarCategorias();
        }
    }
}