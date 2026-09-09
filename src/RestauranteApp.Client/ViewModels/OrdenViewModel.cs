using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RestauranteApp.Client.Services;
using RestauranteApp.Shared.Dtos;
using RestauranteApp.Shared.Enums;

namespace RestauranteApp.Client.ViewModels;

/// <summary>
/// Pantalla táctil de toma de orden: el mesero navega el menú por categorías con el dedo,
/// arma un carrito local y presiona "Enviar a cocina". Ese botón es el flujo principal
/// del sistema: dispara EnviarOrdenAsync, que en el servidor guarda en BD e imprime la(s)
/// comanda(s) en la misma operación. También permite cobrar directo desde aquí para el MVP.
/// </summary>
public partial class OrdenViewModel : ObservableObject
{
    private readonly ApiClient _apiClient;
    private readonly LoginResultado _mesero;
    private readonly int _mesaId;

    private OrdenDto? _ordenActual;

    public event EventHandler? VolverAMapa;

    public ObservableCollection<CategoriaMenuDto> Categorias { get; } = new();
    public ObservableCollection<ItemCarritoViewModel> Carrito { get; } = new();

    [ObservableProperty]
    private CategoriaMenuDto? categoriaSeleccionada;

    [ObservableProperty]
    private bool cargando;

    [ObservableProperty]
    private string? mensajeEstado;

    [ObservableProperty]
    private decimal totalCarrito;

    public OrdenViewModel(ApiClient apiClient, LoginResultado mesero, int mesaId)
    {
        _apiClient = apiClient;
        _mesero = mesero;
        _mesaId = mesaId;

        Carrito.CollectionChanged += (_, _) => RecalcularTotal();
        _ = CargarMenuAsync();
    }

    private async Task CargarMenuAsync()
    {
        Cargando = true;
        try
        {
            var categorias = await _apiClient.ObtenerMenuAsync();
            Application.Current.Dispatcher.Invoke(() =>
            {
                Categorias.Clear();
                foreach (var c in categorias) Categorias.Add(c);
                CategoriaSeleccionada = Categorias.FirstOrDefault();
            });
        }
        finally
        {
            Cargando = false;
        }
    }

    [RelayCommand]
    private void SeleccionarCategoria(CategoriaMenuDto categoria) => CategoriaSeleccionada = categoria;

    [RelayCommand]
    private void AgregarAlCarrito(ItemMenuDto item)
    {
        var existente = Carrito.FirstOrDefault(i => i.ItemMenu.Id == item.Id && string.IsNullOrEmpty(i.Notas));
        if (existente is not null)
        {
            existente.Cantidad++;
        }
        else
        {
            var nuevo = new ItemCarritoViewModel(item);
            nuevo.PropertyChanged += (_, _) => RecalcularTotal();
            Carrito.Add(nuevo);
        }

        RecalcularTotal();
    }

    [RelayCommand]
    private void QuitarDelCarrito(ItemCarritoViewModel item) => Carrito.Remove(item);

    private void RecalcularTotal() => TotalCarrito = Carrito.Sum(i => i.Importe);

    /// <summary>
    /// "Enviar a cocina": guarda los ítems nuevos del carrito en la orden de la mesa y,
    /// en el servidor, dispara la impresión automática en la estación correspondiente.
    /// Si la orden fue modificada por otra terminal mientras tanto, se recarga y se
    /// le pide al mesero reintentar (bloqueo optimista).
    /// </summary>
    [RelayCommand]
    private async Task EnviarACocinaAsync()
    {
        if (Carrito.Count == 0)
        {
            MensajeEstado = "Agrega al menos un platillo antes de enviar.";
            return;
        }

        var itemsRequest = Carrito
            .Select(i => new NuevoItemOrdenRequest(i.ItemMenu.Id, i.Cantidad, i.Notas, Array.Empty<int>()))
            .ToList();

        var request = new EnviarOrdenRequest(_ordenActual?.Id, _mesaId, _mesero.MeseroId, itemsRequest, _ordenActual?.RowVersion);

        try
        {
            Cargando = true;
            var resultado = await _apiClient.EnviarOrdenAsync(request);
            _ordenActual = resultado.Orden;
            Carrito.Clear();
            MensajeEstado = $"Orden enviada a cocina/bar (folio {resultado.Orden.Folio}). Comandas impresas: {resultado.ComandasGeneradas.Count}.";
        }
        catch (InvalidOperationException ex)
        {
            // Incluye el caso de conflicto de concurrencia: se le pide al mesero reintentar.
            MensajeEstado = ex.Message;
        }
        finally
        {
            Cargando = false;
        }
    }

    [RelayCommand]
    private async Task CobrarAsync()
    {
        if (_ordenActual is null)
        {
            MensajeEstado = "Primero envía la orden a cocina antes de cobrar.";
            return;
        }

        try
        {
            Cargando = true;
            var ticket = await _apiClient.CerrarCuentaAsync(_ordenActual.Id, MetodoPago.Efectivo, propina: 0);
            MensajeEstado = $"Cuenta cobrada. Total: ${ticket.Total:0.00}. Ticket enviado a la impresora de caja.";
            VolverAMapa?.Invoke(this, EventArgs.Empty);
        }
        catch (InvalidOperationException ex)
        {
            MensajeEstado = ex.Message;
        }
        finally
        {
            Cargando = false;
        }
    }

    [RelayCommand]
    private void Volver() => VolverAMapa?.Invoke(this, EventArgs.Empty);
}
