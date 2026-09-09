namespace RestauranteApp.Domain.Exceptions;

/// <summary>
/// Se lanza cuando dos usuarios intentan modificar la misma orden/entidad al mismo tiempo
/// (bloqueo optimista). El cliente debe recargar la orden y reintentar con el RowVersion nuevo.
/// </summary>
public class ConflictoConcurrenciaException : Exception
{
    public ConflictoConcurrenciaException(string mensaje) : base(mensaje) { }

    public ConflictoConcurrenciaException(string mensaje, Exception inner) : base(mensaje, inner) { }
}
