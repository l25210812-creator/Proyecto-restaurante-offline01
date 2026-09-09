namespace RestauranteApp.Shared.Enums;

/// <summary>Estado individual de cada ítem dentro de una orden (lo que ve cocina/bar).</summary>
public enum EstadoItemOrden
{
    Pendiente = 0,
    EnPreparacion = 1,
    Listo = 2,
    Entregado = 3,
    Cancelado = 4
}
