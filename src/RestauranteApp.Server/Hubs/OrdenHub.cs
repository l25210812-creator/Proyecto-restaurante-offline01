using Microsoft.AspNetCore.SignalR;
using RestauranteApp.Shared.Realtime;

namespace RestauranteApp.Server.Hubs;

/// <summary>
/// Hub central de tiempo real: todas las terminales (mesero, cocina, bar, caja) se
/// conectan aquí vía LAN. Se tipa contra IOrdenHubClient (definido en Shared) para que
/// el servidor solo pueda invocar los eventos ya acordados con el cliente.
///
/// El Hub en sí no contiene lógica de negocio -- solo agrupa conexiones por "estación"
/// (grupos de SignalR) para poder dirigir notificaciones (ej. solo a cocina) en vez de
/// hacer siempre broadcast a todas las terminales.
/// </summary>
public class OrdenHub : Hub<IOrdenHubClient>
{
    public const string GrupoCocina = "estacion-cocina";
    public const string GrupoBar = "estacion-bar";
    public const string GrupoCaja = "estacion-caja";
    public const string GrupoMeseros = "meseros";

    /// <summary>Cada terminal, al conectarse, indica a qué grupo(s) pertenece según su rol.</summary>
    public async Task UnirseAGrupo(string nombreGrupo)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, nombreGrupo);
    }

    public async Task SalirDeGrupo(string nombreGrupo)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, nombreGrupo);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // SignalR limpia los grupos automáticamente al desconectar; se deja el hook
        // disponible por si en una fase futura se necesita registrar reconexiones.
        await base.OnDisconnectedAsync(exception);
    }
}
