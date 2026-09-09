namespace RestauranteApp.Domain.Common;

/// <summary>
/// Base para todas las entidades persistidas. RowVersion habilita bloqueo optimista
/// en EF Core (columna rowversion/timestamp): si dos meseros intentan guardar la misma
/// orden a la vez, el segundo guardado falla con DbUpdateConcurrencyException en vez de
/// pisar silenciosamente los cambios del primero.
/// </summary>
public abstract class EntidadBase
{
    public int Id { get; set; }

    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
