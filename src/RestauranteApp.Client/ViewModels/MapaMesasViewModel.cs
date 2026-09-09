using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RestauranteApp.Client.Services;
using RestauranteApp.Shared.Dtos;
using RestauranteApp.Shared.Enums;

namespace RestauranteApp.Client.ViewModels;

/// <summary>
/// Mapa de mesas por zona con estado en vivo (libre/ocupada/tiempo transcurrido), tocando
/// una mesa el mesero abre o continúa su orden. Se suscribe a RealtimeClient para reflejar
/// cambios que hagan OTRAS terminales (ej. otro mesero cerró una cuenta) sin refrescar manual.
/// </summary>
public partial class MapaMesasViewModel : ObservableObject, IDisposable
{
    private readonly ApiClient _apiClient;
    private readonly RealtimeClient _realtimeClient;
    private readonly LoginResultado _meseroActual;

    public event EventHandler<int>? MesaSeleccionada;
    public event EventHandler? SolicitarLogout;

    public ObservableCollection<MesaDto> Mesas { get; } = new();

    [ObservableProperty]
    private string nombreMesero = string.Empty;

    [ObservableProperty]
    private bool cargando;

    public MapaMesasViewModel(ApiClient apiClient, RealtimeClient realtimeClient, LoginResultado meseroActual)
    {
        _apiClient = apiClient;
        _realtimeClient = realtimeClient;
        _meseroActual = meseroActual;
        NombreMesero = meseroActual.Nombre;

        _realtimeClient.MesaActualizada += OnMesaActualizada;

        _ = CargarAsync();
        _ = _realtimeClient.ConectarAsync(grupo: "meseros");
    }

    private async Task CargarAsync()
    {
        Cargando = true;
        try
        {
            var mesas = await _apiClient.ObtenerMesasAsync();
            Application.Current.Dispatcher.Invoke(() =>
            {
                Mesas.Clear();
                foreach (var mesa in mesas.OrderBy(m => m.ZonaNombre).ThenBy(m => m.Nombre))
                    Mesas.Add(mesa);
            });
        }
        finally
        {
            Cargando = false;
        }
    }

    private void OnMesaActualizada(object? sender, MesaDto mesaActualizada)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var index = Mesas.ToList().FindIndex(m => m.Id == mesaActualizada.Id);
            if (index >= 0)
                Mesas[index] = mesaActualizada;
            else
                Mesas.Add(mesaActualizada);
        });
    }

    [RelayCommand]
    private void SeleccionarMesa(MesaDto mesa)
    {
        if (mesa.Estado == EstadoMesa.Reservada)
            return; // en el MVP no se abre orden directo sobre una mesa reservada de otro grupo

        MesaSeleccionada?.Invoke(this, mesa.Id);
    }

    [RelayCommand]
    private async Task RefrescarAsync() => await CargarAsync();

    [RelayCommand]
    private void CerrarSesion() => SolicitarLogout?.Invoke(this, EventArgs.Empty);

    public void Dispose()
    {
        _realtimeClient.MesaActualizada -= OnMesaActualizada;
    }
}
