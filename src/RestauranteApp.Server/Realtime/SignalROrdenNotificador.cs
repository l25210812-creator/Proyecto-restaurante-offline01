using Microsoft.AspNetCore.SignalR;
using RestauranteApp.Domain.Entities;
using RestauranteApp.Domain.Interfaces;
using RestauranteApp.Server.Hubs;
using RestauranteApp.Server.Mapping;
using RestauranteApp.Shared.Enums;
using RestauranteApp.Shared.Realtime;

namespace RestauranteApp.Server.Realtime;

/// <summary>
/// Implementación de INotificadorTiempoReal (contrato de Domain) usando IHubContext&lt;OrdenHub&gt;.
/// Aquí es donde de verdad se toca SignalR; Domain solo conoce la interfaz.
/// </summary>
public class SignalROrdenNotificador : INotificadorTiempoReal
{
    private readonly IHubContext<OrdenHub, IOrdenHubClient> _hub;

    public SignalROrdenNotificador(IHubContext<OrdenHub, IOrdenHubClient> hub)
    {
        _hub = hub;
    }

    public Task NotificarMesaActualizadaAsync(Mesa mesa, CancellationToken ct = default) =>
        _hub.Clients.All.MesaActualizada(mesa.ToDto());

    public Task NotificarComandaRecibidaAsync(Comanda comanda, CancellationToken ct = default)
    {
        var grupo = comanda.Estacion == EstacionPreparacion.Cocina ? OrdenHub.GrupoCocina : OrdenHub.GrupoBar;
        return _hub.Clients.Group(grupo).ComandaRecibida(comanda.ToDto());
    }

    public Task NotificarOrdenActualizadaAsync(Orden orden, CancellationToken ct = default) =>
        _hub.Clients.Group(OrdenHub.GrupoMeseros).OrdenActualizada(orden.ToDto());

    public Task NotificarItemOrdenActualizadoAsync(ItemOrden item, CancellationToken ct = default) =>
        _hub.Clients.All.ItemOrdenActualizado(item.OrdenId, item.ToDto());

    public Task NotificarCuentaCerradaAsync(int ordenId, int mesaId, CancellationToken ct = default) =>
        _hub.Clients.All.CuentaCerrada(ordenId, mesaId);

    public Task NotificarAlertaAsync(string mensaje, string severidad, CancellationToken ct = default) =>
        _hub.Clients.All.AlertaSistema(mensaje, severidad);
}
