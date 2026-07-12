using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using MealPlanner.Data;
using MealPlanner.Models;
using MaterialDesignThemes.Wpf;

namespace MealPlanner.Views
{
    public class CalendarioDiaViewModel
    {
        public DateTime Fecha { get; set; }
        public int DiaNumero => Fecha.Day;
        public bool IsCurrentMonth { get; set; }
        public bool IsHoy => Fecha.Date == DateTime.Today;
        public ObservableCollection<CalendarioComidaViewModel> Comidas { get; set; } = new ObservableCollection<CalendarioComidaViewModel>();
    }

    public class CalendarioComidaViewModel
    {
        public string TiempoComida { get; set; } = string.Empty;
        public string NombreComida { get; set; } = string.Empty;
        public string InfoCorto => $"{(TiempoComida.Length >= 3 ? TiempoComida.Substring(0, 3) : TiempoComida)}: {NombreComida}";
        public string InfoLargo => $"{TiempoComida}: {NombreComida}";

        public System.Windows.Media.Brush BackgroundColor
        {
            get
            {
                return TiempoComida switch
                {
                    "Desayuno" => System.Windows.Media.Brushes.MediumSeaGreen,
                    "Almuerzo" => System.Windows.Media.Brushes.DarkOrange,
                    "Merienda" => System.Windows.Media.Brushes.SkyBlue,
                    "Cena" => System.Windows.Media.Brushes.HotPink,
                    _ => System.Windows.Media.Brushes.Gray
                };
            }
        }
    }

    public partial class PlanificacionView : UserControl
    {
        private DateTime _currentMonth;

        public PlanificacionView()
        {
            InitializeComponent();
            _currentMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            Loaded += PlanificacionView_Loaded;
        }

        private void PlanificacionView_Loaded(object sender, RoutedEventArgs e)
        {
            GenerarCalendario();
        }

        private void GenerarCalendario()
        {
            txtMesAnio.Text = _currentMonth.ToString("MMMM yyyy", CultureInfo.CurrentCulture).ToUpper();

            var dias = new ObservableCollection<CalendarioDiaViewModel>();

            // Find the first day of the week for the first day of the month (Monday = 1)
            int firstDayOfWeek = (int)_currentMonth.DayOfWeek;
            if (firstDayOfWeek == 0) firstDayOfWeek = 7; // Sunday

            // Backtrack to Monday
            DateTime startDate = _currentMonth.AddDays(-(firstDayOfWeek - 1));

            // Load all planificacion items for the 6 weeks shown
            DateTime endDate = startDate.AddDays(42);
            List<PlanificacionItem> itemsMes;

            using (var context = new AppDbContext())
            {
                itemsMes = context.PlanificacionItems
                    .Include(p => p.Comida)
                    .Where(p => p.Fecha >= startDate && p.Fecha < endDate)
                    .ToList();
            }

            var orderDict = new Dictionary<string, int> { { "Desayuno", 1 }, { "Almuerzo", 2 }, { "Merienda", 3 }, { "Cena", 4 } };

            for (int i = 0; i < 42; i++)
            {
                DateTime d = startDate.AddDays(i);
                var diaVm = new CalendarioDiaViewModel
                {
                    Fecha = d,
                    IsCurrentMonth = d.Month == _currentMonth.Month
                };

                var itemsDia = itemsMes
                    .Where(p => p.Fecha.Date == d.Date)
                    .OrderBy(p => orderDict.ContainsKey(p.TiempoComida) ? orderDict[p.TiempoComida] : 99)
                    .ToList();

                foreach (var item in itemsDia)
                {
                    diaVm.Comidas.Add(new CalendarioComidaViewModel
                    {
                        TiempoComida = item.TiempoComida,
                        NombreComida = item.Comida?.Nombre ?? "Desconocido"
                    });
                }

                dias.Add(diaVm);
            }

            icCalendario.ItemsSource = dias;
        }

        private void btnMesAnterior_Click(object sender, RoutedEventArgs e)
        {
            _currentMonth = _currentMonth.AddMonths(-1);
            GenerarCalendario();
        }

        private void btnMesSiguiente_Click(object sender, RoutedEventArgs e)
        {
            _currentMonth = _currentMonth.AddMonths(1);
            GenerarCalendario();
        }

        private void btnHoy_Click(object sender, RoutedEventArgs e)
        {
            _currentMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            GenerarCalendario();
        }

        private async void BorderDia_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is CalendarioDiaViewModel diaVm)
            {
                await MostrarDetalleYAsignacion(diaVm.Fecha);
            }
        }

        private async System.Threading.Tasks.Task MostrarDetalleYAsignacion(DateTime fecha)
        {
            var detalleDialog = new DetalleDiaDialog(fecha);
            var result = await DialogHost.Show(detalleDialog, "RootDialog");

            // If the user closed or deleted items, we might need to refresh
            if (detalleDialog.NeedsRefresh || (result as string == "CERRAR"))
            {
                GenerarCalendario();
            }

            // If the user clicked "+ AGREGAR COMIDA"
            if (result as string == "AGREGAR")
            {
                var asignarDialog = new AsignarComidaDialog(fecha);
                var addResult = await DialogHost.Show(asignarDialog, "RootDialog");

                if (addResult is bool wasAdded && wasAdded)
                {
                    GenerarCalendario();
                }

                // Re-open detail dialog so they can see the new item or continue managing the day
                await MostrarDetalleYAsignacion(fecha);
            }
        }
    }
}