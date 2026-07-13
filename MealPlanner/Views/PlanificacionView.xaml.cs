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
                    "Desayuno" => new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#9E9820")), // Olive
                    "Almuerzo" => new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FC8A2D")), // Princeton Orange
                    "Merienda" => new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#C42B34")), // Tomato Jam
                    "Cena" => new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#DC4F7C")), // Blush Rose
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
            if (cmbVista.SelectedIndex == 1) // Semanal
            {
                _currentMonth = _currentMonth.AddDays(-7);
                GenerarCalendarioSemanal();
            }
            else
            {
                _currentMonth = _currentMonth.AddMonths(-1);
                GenerarCalendario();
            }
        }

        private void btnMesSiguiente_Click(object sender, RoutedEventArgs e)
        {
            if (cmbVista.SelectedIndex == 1) // Semanal
            {
                _currentMonth = _currentMonth.AddDays(7);
                GenerarCalendarioSemanal();
            }
            else
            {
                _currentMonth = _currentMonth.AddMonths(1);
                GenerarCalendario();
            }
        }

        private void btnHoy_Click(object sender, RoutedEventArgs e)
        {
            _currentMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            if (cmbVista.SelectedIndex == 1)
            {
                _currentMonth = DateTime.Today; // Si es semana, saltar a la semana actual
                GenerarCalendarioSemanal();
            }
            else
            {
                GenerarCalendario();
            }
        }

        private async void BorderDia_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is CalendarioDiaViewModel diaVm)
            {
                await MostrarDetalleYAsignacion(diaVm.Fecha);
            }
        }

        private void cmbVista_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GridVistaMensual == null || GridVistaSemanal == null) return;

            bool esSemana = (cmbVista.SelectedIndex == 1);
            if (esSemana)
            {
                GridVistaMensual.Visibility = Visibility.Collapsed;
                GridVistaSemanal.Visibility = Visibility.Visible;
                GenerarCalendarioSemanal();
            }
            else
            {
                GridVistaSemanal.Visibility = Visibility.Collapsed;
                GridVistaMensual.Visibility = Visibility.Visible;
                GenerarCalendario();
            }
        }

        private void GenerarCalendarioSemanal()
        {
            gridLayoutSemana.Children.Clear();
            gridLayoutSemana.RowDefinitions.Clear();
            gridLayoutSemana.ColumnDefinitions.Clear();

            // 8 Columns: 1 for headers + 7 for days
            for (int i = 0; i < 8; i++)
            {
                gridLayoutSemana.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            }

            // 5 Rows: 1 for day headers + 4 for meal times
            for (int i = 0; i < 5; i++)
            {
                gridLayoutSemana.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            }

            // Set Title Month/Year based on the week
            txtMesAnio.Text = $"Semana del {_currentMonth.ToString("dd MMM yyyy", CultureInfo.CurrentCulture).ToUpper()}";

            int firstDayOfWeek = (int)_currentMonth.DayOfWeek;
            if (firstDayOfWeek == 0) firstDayOfWeek = 7; // Sunday to 7
            DateTime startDate = _currentMonth.AddDays(-(firstDayOfWeek - 1));

            List<PlanificacionItem> itemsSemana;
            DateTime endDate = startDate.AddDays(7);

            using (var context = new AppDbContext())
            {
                itemsSemana = context.PlanificacionItems
                    .Include(p => p.Comida)
                    .Where(p => p.Fecha >= startDate && p.Fecha < endDate)
                    .ToList();
            }

            // Draw Day Headers (Row 0, Cols 1-7)
            string[] diasNombres = { "Lun", "Mar", "Mié", "Jue", "Vie", "Sáb", "Dom" };
            for (int i = 0; i < 7; i++)
            {
                DateTime d = startDate.AddDays(i);
                var tb = new TextBlock
                {
                    Text = $"{diasNombres[i]} {d.Day}",
                    FontWeight = (d.Date == DateTime.Today) ? FontWeights.Bold : FontWeights.Normal,
                    TextAlignment = TextAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetRow(tb, 0);
                Grid.SetColumn(tb, i + 1);
                gridLayoutSemana.Children.Add(tb);
            }

            // Meal Times Configuration
            var tiempos = new List<(string Name, System.Windows.Media.Brush Color)>
            {
                ("Desayuno", new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#9E9820"))),
                ("Almuerzo", new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FC8A2D"))),
                ("Merienda", new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#C42B34"))),
                ("Cena", new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#DC4F7C")))
            };

            for (int row = 0; row < tiempos.Count; row++)
            {
                int actualRow = row + 1;

                // Row Header
                var tbRow = new TextBlock
                {
                    Text = tiempos[row].Name,
                    FontWeight = FontWeights.Bold,
                    TextAlignment = TextAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = tiempos[row].Color
                };
                Grid.SetRow(tbRow, actualRow);
                Grid.SetColumn(tbRow, 0);
                gridLayoutSemana.Children.Add(tbRow);

                // Grid Cells
                for (int col = 1; col <= 7; col++)
                {
                    DateTime d = startDate.AddDays(col - 1);

                    var borderCell = new Border
                    {
                        BorderThickness = new Thickness(0.5),
                        BorderBrush = Application.Current.TryFindResource("MaterialDesignDivider") as System.Windows.Media.Brush ?? System.Windows.Media.Brushes.Gray,
                        Background = System.Windows.Media.Brushes.Transparent,
                        Cursor = Cursors.Hand,
                        Margin = new Thickness(1),
                        ClipToBounds = true
                    };

                    // Add click event dynamically
                    borderCell.MouseLeftButtonUp += async (s, e) => { await MostrarDetalleYAsignacion(d); };

                    var cellItems = itemsSemana
                        .Where(p => p.Fecha.Date == d.Date && p.TiempoComida == tiempos[row].Name)
                        .ToList();

                    var stackPanel = new StackPanel { Margin = new Thickness(2) };

                    foreach(var item in cellItems)
                    {
                        var borderMeal = new Border
                        {
                            Background = tiempos[row].Color,
                            CornerRadius = new CornerRadius(2),
                            Margin = new Thickness(0,2,0,0),
                            Padding = new Thickness(2)
                        };

                        var tbMeal = new TextBlock
                        {
                            Text = item.Comida?.Nombre,
                            FontSize = 10,
                            Foreground = System.Windows.Media.Brushes.White,
                            TextTrimming = TextTrimming.CharacterEllipsis,
                            ToolTip = item.Comida?.Nombre
                        };

                        borderMeal.Child = tbMeal;
                        stackPanel.Children.Add(borderMeal);
                    }

                    borderCell.Child = stackPanel;

                    Grid.SetRow(borderCell, actualRow);
                    Grid.SetColumn(borderCell, col);
                    gridLayoutSemana.Children.Add(borderCell);
                }
            }
        }

        private async System.Threading.Tasks.Task MostrarDetalleYAsignacion(DateTime fecha)
        {
            var detalleDialog = new DetalleDiaDialog(fecha);
            var result = await DialogHost.Show(detalleDialog, "RootDialog");

            // If the user closed or deleted items, we might need to refresh
            if (detalleDialog.NeedsRefresh || (result as string == "CERRAR"))
            {
                RefrescarVistaActual();
            }

            // If the user clicked "+ AGREGAR COMIDA"
            if (result as string == "AGREGAR")
            {
                var asignarDialog = new AsignarComidaDialog(fecha);
                var addResult = await DialogHost.Show(asignarDialog, "RootDialog");

                if (addResult is bool wasAdded && wasAdded)
                {
                    RefrescarVistaActual();
                }

                // Re-open detail dialog so they can see the new item or continue managing the day
                await MostrarDetalleYAsignacion(fecha);
            }
        }

        private void RefrescarVistaActual()
        {
            if (cmbVista != null && cmbVista.SelectedIndex == 1)
            {
                GenerarCalendarioSemanal();
            }
            else
            {
                GenerarCalendario();
            }
        }
    }
}