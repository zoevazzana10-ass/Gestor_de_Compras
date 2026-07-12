using System.Windows;
using System.Windows.Controls;
using MealPlanner.Views;

namespace MealPlanner;

public partial class MainWindow : Window
{
    private PlanificacionView _planificacionView;
    private ComidasView _comidasView;
    private RecetasView _recetasView;
    private IngredientesView _ingredientesView;
    private ListaComprasView _listaComprasView;

    public MainWindow()
    {
        InitializeComponent();

        _planificacionView = new PlanificacionView();
        _comidasView = new ComidasView();
        _recetasView = new RecetasView();
        _ingredientesView = new IngredientesView();
        _listaComprasView = new ListaComprasView();

        MainContentControl.Content = _planificacionView;
    }

    private void MenuListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (MainContentControl == null) return;

        switch (MenuListBox.SelectedIndex)
        {
            case 0:
                MainContentControl.Content = _planificacionView;
                break;
            case 1:
                MainContentControl.Content = _comidasView;
                break;
            case 2:
                MainContentControl.Content = _recetasView;
                break;
            case 3:
                MainContentControl.Content = _ingredientesView;
                break;
            case 4:
                MainContentControl.Content = _listaComprasView;
                break;
        }

        if (MainDrawer != null)
        {
            MainDrawer.IsLeftDrawerOpen = false;
            if (MenuToggleButton != null)
            {
                MenuToggleButton.IsChecked = false;
            }
        }
    }
}