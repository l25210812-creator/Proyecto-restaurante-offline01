using System.Net.Http.Json;
using RestauranteApp.Shared.Dtos;

namespace RestauranteApp.Client.Services;

public record LoginResultado(int MeseroId, string Nombre, string Rol);
public record LoginPeticion(int NumeroMesero, string Pin);

/// <summary>
/// Cliente HTTP hacia RestauranteApp.Server. Encapsula las rutas REST del MVP; las
/// actualizaciones en vivo (comandas, mesas, órdenes) llegan aparte vía RealtimeClient (SignalR).
/// </summary>
public class ApiClient
{
    private readonly HttpClient _http;

    public ApiClient(string baseUrl)
    {
        _http = new HttpClient { BaseAddress = new Uri(baseUrl), Timeout = TimeSpan.FromSeconds(10) };
    }

    public async Task<LoginResultado> LoginAsync(int numeroMesero, string pin, CancellationToken ct = default)
    {
        var respuesta = await _http.PostAsJsonAsync("/api/auth/login", new LoginPeticion(numeroMesero, pin), ct);
        if (!respuesta.IsSuccessStatusCode)
            throw new InvalidOperationException("Número de mesero o PIN incorrectos.");

        return (await respuesta.Content.ReadFromJsonAsync<LoginResultado>(cancellationToken: ct))!;
    }

    public async Task<IReadOnlyList<MesaDto>> ObtenerMesasAsync(CancellationToken ct = default) =>
        await _http.GetFromJsonAsync<List<MesaDto>>("/api/mesas", ct) ?? new List<MesaDto>();

    public async Task<IReadOnlyList<CategoriaMenuDto>> ObtenerMenuAsync(CancellationToken ct = default) =>
        await _http.GetFromJsonAsync<List<CategoriaMenuDto>>("/api/menu", ct) ?? new List<CategoriaMenuDto>();

    /// <summary>
    /// El botón "Enviar a cocina" de la terminal del mesero llama esto: el servidor guarda
    /// la orden en base de datos e imprime la(s) comanda(s) en la misma operación.
    /// </summary>
    public async Task<EnviarOrdenResultado> EnviarOrdenAsync(EnviarOrdenRequest request, CancellationToken ct = default)
    {
        var respuesta = await _http.PostAsJsonAsync("/api/ordenes/enviar", request, ct);

        if (respuesta.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            var error = await respuesta.Content.ReadFromJsonAsync<MensajeError>(cancellationToken: ct);
            throw new InvalidOperationException(error?.Mensaje ?? "La orden fue modificada por otra terminal. Recárgala e intenta de nuevo.");
        }

        respuesta.EnsureSuccessStatusCode();
        return (await respuesta.Content.ReadFromJsonAsync<EnviarOrdenResultado>(cancellationToken: ct))!;
    }

    public async Task CambiarEstadoItemAsync(int itemOrdenId, Shared.Enums.EstadoItemOrden nuevoEstado, CancellationToken ct = default)
    {
        var respuesta = await _http.PostAsJsonAsync($"/api/ordenes/items/{itemOrdenId}/estado", new { NuevoEstado = nuevoEstado }, ct);
        respuesta.EnsureSuccessStatusCode();
    }

    public async Task<TicketCuentaDto> CerrarCuentaAsync(int ordenId, Shared.Enums.MetodoPago metodo, decimal propina, CancellationToken ct = default)
    {
        var respuesta = await _http.PostAsJsonAsync("/api/cuentas/cerrar", new { OrdenId = ordenId, MetodoPago = metodo, Propina = propina }, ct);
        respuesta.EnsureSuccessStatusCode();
        return (await respuesta.Content.ReadFromJsonAsync<TicketCuentaDto>(cancellationToken: ct))!;
    }

    private record MensajeError(string Mensaje);
}
