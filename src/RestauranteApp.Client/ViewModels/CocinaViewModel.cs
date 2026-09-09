using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RestauranteApp.Client.Services;
using RestauranteApp.Shared.Dtos;
using RestauranteApp.Shared.Enums;

namespace RestauranteApp.Client.ViewModels;

/// <summary>
/// Panel de cocina/bar: muestra las comandas según llegan en tiempo real (SignalR) y
/// permite marcar cada ítem como "Listo" para avisar al mesero. La comanda también se
/// imprimió físicamente en el momento del envío -- esta pantalla es el respaldo digital,
/// no reemplaza el papel.
/// </summary>
public partial class CocinaViewModel : ObservableObject, IDisposable
{
    private readonly ApiClient _apiClient;
    private readonly RealtimeClient _realtimeClient;
    private readonly EstacionPreparacion _estacion;

    // Ids de ítems ya marcados "Listo" en esta sesión de pantalla; los ComandaDto son
    // inmutables (records), así que en vez de mutarlos llevamos aparte qué ya se resolvió.
    private readonly HashSet<int> _itemsListos = new();

    public event EventHandler? SolicitarLogout;

    public ObservableCollection<ComandaDto> Comandas { get; } = new();

    [ObservableProperty]
    private string tituloEstacion = string.Empty;

    public CocinaViewModel(ApiClient apiClient, RealtimeClient realtimeClient, EstacionPreparacion estacion)
    {
        _apiClient = apiClient;
        _realtimeClient = realtimeClient;
        _estacion = estacion;
        TituloEstacion = estacion == EstacionPreparacion.Cocina ? "Panel de cocina" : "Panel de bar";

        _realtimeClient.ComandaRecibida += OnComandaRecibida;

        var grupo = estacion == EstacionPreparacion.Cocina ? "estacion-cocina" : "estacion-bar";
        _ = _realtimeClient.ConectarAsync(grupo);
    }

    private void OnComandaRecibida(object? sender, ComandaDto comanda)
    {
        if (comanda.Estacion != _estacion) return;

        Application.Current.Dispatcher.Invoke(() => Comandas.Add(comanda));
    }

    public bool EstaListo(int itemOrdenId) => _itemsListos.Contains(itemOrdenId);

    [RelayCommand]
    private async Task MarcarListoAsync(ItemOrdenDto item)
    {
        await _apiClient.CambiarEstadoItemAsync(item.Id, EstadoItemOrden.Listo);
        _itemsListos.Add(item.Id);

        Application.Current.Dispatcher.Invoke(() =>
        {
            var comanda = Comandas.FirstOrDefault(c => c.Items.Any(i => i.Id == item.Id));
            if (comanda is not null && comanda.Items.All(i => _itemsListos.Contains(i.Id)))
                Comandas.Remove(comanda);
        });
    }

    [RelayCommand]
    private void CerrarSesion() => SolicitarLogout?.Invoke(this, EventArgs.Empty);

    public void Dispose()
    {
        _realtimeClient.ComandaRecibida -= OnComandaRecibida;
    }
}
