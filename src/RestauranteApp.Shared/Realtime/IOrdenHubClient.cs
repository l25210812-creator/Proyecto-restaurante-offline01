using RestauranteApp.Shared.Dtos;

namespace RestauranteApp.Shared.Realtime;

/// <summary>
/// Contrato de los eventos que el servidor empuja a los clientes conectados por SignalR.
/// El Hub del servidor se declara como Hub&lt;IOrdenHubClient&gt; y el cliente WPF genera
/// un HubConnection y usa connection.On&lt;...&gt;(nameof(...)) con estos mismos nombres,
/// de forma que ambos lados quedan sincronizados por el contrato compartido.
/// </summary>
public interface IOrdenHubClient
{
    /// <summary>Una mesa cambió de estado (libre/ocupada/etc.) — refresca el mapa de mesas.</summary>
    Task MesaActualizada(MesaDto mesa);

    /// <summary>Se envió una comanda nueva a una estación — la pantalla de cocina/bar debe mostrarla.</summary>
    Task ComandaRecibida(ComandaDto comanda);

    /// <summary>Un ítem cambió de estado (en preparación, listo, entregado).</summary>
    Task ItemOrdenActualizado(int ordenId, ItemOrdenDto item);

    /// <summary>La orden completa de una mesa se actualizó (útil para refrescar la vista del mesero).</summary>
    Task OrdenActualizada(OrdenDto orden);

    /// <summary>Se cerró/cobró una cuenta — cocina y meseros pueden liberar la mesa en su vista.</summary>
    Task CuentaCerrada(int ordenId, int mesaId);

    /// <summary>Aviso operativo (ej. "impresora de cocina sin papel") para mostrar en pantalla.</summary>
    Task AlertaSistema(string mensaje, string severidad);
}
