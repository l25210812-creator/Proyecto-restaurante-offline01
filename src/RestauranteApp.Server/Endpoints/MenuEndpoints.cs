using RestauranteApp.Domain.Interfaces;
using RestauranteApp.Server.Mapping;

namespace RestauranteApp.Server.Endpoints;

public static class MenuEndpoints
{
    public static void MapMenuEndpoints(this WebApplication app)
    {
        app.MapGet("/api/menu", async (IUnitOfWork unitOfWork, CancellationToken ct) =>
        {
            var categorias = await unitOfWork.Menu.ObtenerMenuCompletoAsync(ct);
            return Results.Ok(categorias.Select(c => c.ToDto()));
        }).WithTags("Menú");
    }
}
