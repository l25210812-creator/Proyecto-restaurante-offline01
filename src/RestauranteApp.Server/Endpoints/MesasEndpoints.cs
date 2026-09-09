using RestauranteApp.Domain.Services;
using RestauranteApp.Server.Mapping;

namespace RestauranteApp.Server.Endpoints;

public static class MesasEndpoints
{
    public static void MapMesasEndpoints(this WebApplication app)
    {
        var grupo = app.MapGroup("/api/mesas").WithTags("Mesas");

        grupo.MapGet("/", async (MesaService mesaService, CancellationToken ct) =>
        {
            var mesas = await mesaService.ObtenerMapaDeMesasAsync(ct);
            return Results.Ok(mesas.Select(m => m.ToDto()));
        });

        grupo.MapPost("/{mesaId:int}/marcar-limpia", async (int mesaId, MesaService mesaService, CancellationToken ct) =>
        {
            await mesaService.MarcarLimpiaAsync(mesaId, ct);
            return Results.NoContent();
        });
    }
}
