using RestauranteApp.Domain.Entities;

namespace RestauranteApp.Domain.Interfaces;

/// <summary>
/// Abstracción sobre SignalR. La implementación real (Server/Realtime/SignalROrdenNotificador)
/// envuelve IHubContext&lt;OrdenHub&gt;; Domain solo necesita saber que puede avisar a las
/// terminales conectadas, sin depender de ASP.NET Core SignalR directamente.
/// </summary>
public interface INotificadorTiempoReal
{
    Task NotificarMesaActualizadaAsync(Mesa mesa, CancellationToken ct = default);
    Task NotificarComandaRecibidaAsync(Comanda comanda, CancellationToken ct = default);
    Task NotificarOrdenActualizadaAsync(Orden orden, CancellationToken ct = default);
    Task NotificarItemOrdenActualizadoAsync(ItemOrden item, CancellationToken ct = default);
    Task NotificarCuentaCerradaAsync(int ordenId, int mesaId, CancellationToken ct = default);
    Task NotificarAlertaAsync(string mensaje, string severidad, CancellationToken ct = default);
}
