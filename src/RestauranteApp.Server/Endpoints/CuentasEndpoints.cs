using RestauranteApp.Domain.Services;
using RestauranteApp.Server.Mapping;
using RestauranteApp.Shared.Enums;

namespace RestauranteApp.Server.Endpoints;

public record CerrarCuentaRequest(int OrdenId, MetodoPago MetodoPago, decimal Propina);

public static class CuentasEndpoints
{
    public static void MapCuentasEndpoints(this WebApplication app)
    {
        var grupo = app.MapGroup("/api/cuentas").WithTags("Cuentas");

        // Cobro: genera el total y dispara la impresión del ticket final en caja,
        // que es el documento físico que se entrega al cliente.
        grupo.MapPost("/cerrar", async (CerrarCuentaRequest req, CuentaService cuentaService, CancellationToken ct) =>
        {
            try
            {
                var cuenta = await cuentaService.CerrarCuentaAsync(req.OrdenId, req.MetodoPago, req.Propina, ct);
                return Results.Ok(cuenta.ToTicketDto());
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { Mensaje = ex.Message });
            }
        });
    }
}
