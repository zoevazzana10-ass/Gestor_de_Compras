using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MealPlanner.Data;
using MealPlanner.Models;

namespace MealPlanner.Views
{
    public partial class IngredientesView : UserControl
    {
        public IngredientesView()
        {
            InitializeComponent();
            Loaded += IngredientesView_Loaded;
        }

        private void IngredientesView_Loaded(object sender, RoutedEventArgs e)
        {
            CargarIngredientes();
        }

        private void CargarIngredientes()
        {
            using (var context = new AppDbContext())
            {
                context.Database.EnsureCreated();
                dgIngredientes.ItemsSource = context.Ingredientes.ToList();
            }
        }

        private void btnAgregar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("El nombre es obligatorio");
                return;
            }

            var nuevoIngrediente = new Ingrediente
            {
                Nombre = txtNombre.Text,
                Categoria = txtCategoria.Text,
                DondeSeConsigue = txtDonde.Text,
                UnidadMedida = txtUnidad.Text
            };

            using (var context = new AppDbContext())
            {
                context.Ingredientes.Add(nuevoIngrediente);
                context.SaveChanges();
            }

            txtNombre.Clear();
            txtCategoria.Clear();
            txtDonde.Clear();
            txtUnidad.Clear();

            CargarIngredientes();
        }
    }
}
