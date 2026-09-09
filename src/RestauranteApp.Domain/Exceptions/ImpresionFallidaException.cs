namespace RestauranteApp.Domain.Exceptions;

/// <summary>
/// Señala que una comanda o ticket no pudo imprimirse (impresora apagada/sin papel/sin red).
/// No implica que la orden se haya perdido: el registro en base de datos ya existe y el
/// trabajo de impresión queda en cola de reintentos.
/// </summary>
public class ImpresionFallidaException : Exception
{
    public ImpresionFallidaException(string mensaje) : base(mensaje) { }

    public ImpresionFallidaException(string mensaje, Exception inner) : base(mensaje, inner) { }
}
