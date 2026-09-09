using RestauranteApp.Domain.Services;

namespace RestauranteApp.Server.Endpoints;

public record LoginRequest(int NumeroMesero, string Pin);
public record LoginResponse(int MeseroId, string Nombre, string Rol);

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var grupo = app.MapGroup("/api/auth").WithTags("Autenticación");

        grupo.MapPost("/login", async (LoginRequest req, AutenticacionService autenticacionService, CancellationToken ct) =>
        {
            try
            {
                var mesero = await autenticacionService.LoginAsync(req.NumeroMesero, req.Pin, ct);
                return Results.Ok(new LoginResponse(mesero.Id, mesero.Nombre, mesero.Rol.ToString()));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Results.Json(new { Mensaje = ex.Message }, statusCode: StatusCodes.Status401Unauthorized);
            }
        });
    }
}
