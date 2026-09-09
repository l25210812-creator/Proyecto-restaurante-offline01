namespace RestauranteApp.Shared.Realtime;

/// <summary>Rutas conocidas por ambos lados (servidor las mapea, cliente las usa para conectarse).</summary>
public static class HubRoutes
{
    public const string OrdenHub = "/hubs/ordenes";
}
