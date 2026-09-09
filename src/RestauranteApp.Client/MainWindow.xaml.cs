using System.Windows;
using RestauranteApp.Client.Services;
using RestauranteApp.Client.ViewModels;
using RestauranteApp.Client.Views;
using RestauranteApp.Shared.Enums;

namespace RestauranteApp.Client;

public partial class MainWindow : Window
{
    private readonly ApiClient _apiClient;
    private readonly RealtimeClient _realtimeClient;

    public MainWindow()
    {
        InitializeComponent();

        // MVP: URL del servidor fija/configurable; en una fase futura esto se lee de un
        // archivo de configuración local para no recompilar el cliente por cada terminal.
        var baseUrl = "http://192.168.1.10:5080";
        _apiClient = new ApiClient(baseUrl);
        _realtimeClient = new RealtimeClient(baseUrl);

        MostrarLogin();
    }

    private void MostrarLogin()
    {
        var vm = new LoginViewModel(_apiClient);
        vm.LoginExitoso += (_, mesero) => EnrutarPorRol(mesero);
        ContenidoPrincipal.Content = new LoginView { DataContext = vm };
    }

    /// <summary>
    /// Cada terminal muestra la pantalla adecuada según el rol de quien inició sesión:
    /// mesero/cajero/admin ven el mapa de mesas (touch), cocina/bar ven su panel de comandas.
    /// </summary>
    private void EnrutarPorRol(LoginResultado mesero)
    {
        if (Enum.TryParse<RolUsuario>(mesero.Rol, out var rol) &&
            (rol == RolUsuario.Cocina || rol == RolUsuario.Bar))
        {
            MostrarCocina(mesero, rol == RolUsuario.Cocina ? EstacionPreparacion.Cocina : EstacionPreparacion.Bar);
        }
        else
        {
            MostrarMapaMesas(mesero);
        }
    }

    private void MostrarMapaMesas(LoginResultado mesero)
    {
        var vm = new MapaMesasViewModel(_apiClient, _realtimeClient, mesero);
        vm.MesaSeleccionada += (_, mesaId) => MostrarOrden(mesero, mesaId);
        vm.SolicitarLogout += (_, _) => MostrarLogin();
        ContenidoPrincipal.Content = new MapaMesasView { DataContext = vm };
    }

    private void MostrarOrden(LoginResultado mesero, int mesaId)
    {
        var vm = new OrdenViewModel(_apiClient, mesero, mesaId);
        vm.VolverAMapa += (_, _) => MostrarMapaMesas(mesero);
        ContenidoPrincipal.Content = new OrdenView { DataContext = vm };
    }

    private void MostrarCocina(LoginResultado mesero, EstacionPreparacion estacion)
    {
        var vm = new CocinaViewModel(_apiClient, _realtimeClient, estacion);
        vm.SolicitarLogout += (_, _) => MostrarLogin();
        ContenidoPrincipal.Content = new CocinaView { DataContext = vm };
    }
}
