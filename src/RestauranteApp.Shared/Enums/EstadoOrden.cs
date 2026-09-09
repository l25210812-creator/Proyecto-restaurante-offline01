namespace RestauranteApp.Shared.Enums;

/// <summary>Estado general de una orden abierta en una mesa.</summary>
public enum EstadoOrden
{
    Abierta = 0,
    EnviadaACocina = 1,
    Servida = 2,
    Cobrada = 3,
    Cancelada = 4
}
