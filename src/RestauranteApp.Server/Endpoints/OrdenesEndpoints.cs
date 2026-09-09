using RestauranteApp.Domain.Exceptions;
using RestauranteApp.Domain.Services;
using RestauranteApp.Server.Mapping;
using RestauranteApp.Shared.Dtos;
using RestauranteApp.Shared.Enums;

namespace RestauranteApp.Server.Endpoints;

public record CambiarEstadoItemRequest(EstadoItemOrden NuevoEstado);

public static class OrdenesEndpoints
{
    public static void MapOrdenesEndpoints(this WebApplication app)
    {
        var grupo = app.MapGroup("/api/ordenes").WithTags("Órdenes");

        // Este es el endpoint del flujo principal: el mesero presiona "Enviar a cocina"
        // en su terminal táctil y esto dispara guardado en BD + impresión en la misma operación.
        grupo.MapPost("/enviar", async (EnviarOrdenRequest req, OrdenService ordenService, CancellationToken ct) =>
        {
            try
            {
                var (orden, comandas) = await ordenService.EnviarOrdenAsync(
                    req.MesaId, req.MeseroId, req.Items, req.RowVersion, ct);

                return Results.Ok(new EnviarOrdenResultado(
                    orden.ToDto(),
                    comandas.Select(c => c.ToDto()).ToList()));
            }
            catch (ConflictoConcurrenciaException ex)
            {
                return Results.Conflict(new { Mensaje = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { Mensaje = ex.Message });
            }
        });

        // Cocina/bar usan esto para marcar un ítem en preparación/listo/entregado.
        grupo.MapPost("/items/{itemOrdenId:int}/estado", async (
            int itemOrdenId, CambiarEstadoItemRequest req, OrdenService ordenService, CancellationToken ct) =>
        {
            try
            {
                var item = await ordenService.CambiarEstadoItemAsync(itemOrdenId, req.NuevoEstado, ct);
                return Results.Ok(item.ToDto());
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { Mensaje = ex.Message });
            }
        });

        grupo.MapPost("/{ordenId:int}/traspasar/{mesaDestinoId:int}", async (
            int ordenId, int mesaDestinoId, OrdenService ordenService, CancellationToken ct) =>
        {
            try
            {
                await ordenService.TraspasarMesaAsync(ordenId, mesaDestinoId, ct);
                return Results.NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { Mensaje = ex.Message });
            }
        });
    }
}
