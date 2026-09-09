using Microsoft.AspNetCore.SignalR.Client;
using RestauranteApp.Shared.Dtos;
using RestauranteApp.Shared.Realtime;

namespace RestauranteApp.Client.Services;

/// <summary>
/// Envuelve la conexión SignalR hacia el servidor. Expone eventos .NET normales para que
/// los ViewModels no tengan que conocer HubConnection directamente. Incluye reconexión
/// automática: si una terminal pierde la red momentáneamente (típico en LAN con Wi-Fi),
/// se reconecta sola y vuelve a unirse a sus grupos sin perder estado de la orden en curso.
/// </summary>
public class RealtimeClient
{
    private readonly HubConnection _connection;

    public event EventHandler<MesaDto>? MesaActualizada;
    public event EventHandler<ComandaDto>? ComandaRecibida;
    public event EventHandler<OrdenDto>? OrdenActualizada;
    public event EventHandler<(int OrdenId, ItemOrdenDto Item)>? ItemOrdenActualizado;
    public event EventHandler<(int OrdenId, int MesaId)>? CuentaCerrada;
    public event EventHandler<(string Mensaje, string Severidad)>? AlertaSistema;
    public event EventHandler? Reconectado;

    private string? _grupoActual;

    public RealtimeClient(string baseUrl)
    {
        _connection = new HubConnectionBuilder()
            .WithUrl(new Uri(new Uri(baseUrl), HubRoutes.OrdenHub))
            .WithAutomaticReconnect(new[]
            {
                TimeSpan.Zero, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10)
            })
            .Build();

        _connection.On<MesaDto>(nameof(IOrdenHubClient.MesaActualizada), m => MesaActualizada?.Invoke(this, m));
        _connection.On<ComandaDto>(nameof(IOrdenHubClient.ComandaRecibida), c => ComandaRecibida?.Invoke(this, c));
        _connection.On<OrdenDto>(nameof(IOrdenHubClient.OrdenActualizada), o => OrdenActualizada?.Invoke(this, o));
        _connection.On<int, ItemOrdenDto>(nameof(IOrdenHubClient.ItemOrdenActualizado), (ordenId, item) => ItemOrdenActualizado?.Invoke(this, (ordenId, item)));
        _connection.On<int, int>(nameof(IOrdenHubClient.CuentaCerrada), (ordenId, mesaId) => CuentaCerrada?.Invoke(this, (ordenId, mesaId)));
        _connection.On<string, string>(nameof(IOrdenHubClient.AlertaSistema), (msg, sev) => AlertaSistema?.Invoke(this, (msg, sev)));

        _connection.Reconnected += async _ =>
        {
            if (_grupoActual is not null)
                await _connection.InvokeAsync("UnirseAGrupo", _grupoActual);
            Reconectado?.Invoke(this, EventArgs.Empty);
        };
    }

    public async Task ConectarAsync(string? grupo = null, CancellationToken ct = default)
    {
        if (_connection.State == HubConnectionState.Disconnected)
            await _connection.StartAsync(ct);

        if (grupo is not null)
        {
            _grupoActual = grupo;
            await _connection.InvokeAsync("UnirseAGrupo", grupo, ct);
        }
    }

    public Task DesconectarAsync() => _connection.StopAsync();
}
