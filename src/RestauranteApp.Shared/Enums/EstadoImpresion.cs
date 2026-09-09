namespace RestauranteApp.Shared.Enums;

/// <summary>Estado del intento de impresión de una comanda o ticket.</summary>
public enum EstadoImpresion
{
    Pendiente = 0,
    Impresa = 1,
    Fallida = 2,
    Reintentando = 3,
    Cancelada = 4
}
