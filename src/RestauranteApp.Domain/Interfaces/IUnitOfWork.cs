using RestauranteApp.Domain.Interfaces.Repositories;

namespace RestauranteApp.Domain.Interfaces;

/// <summary>
/// Agrupa los repositorios de una misma transacción y expone GuardarCambiosAsync,
/// que es donde EF Core aplica el chequeo de RowVersion (bloqueo optimista).
/// </summary>
public interface IUnitOfWork
{
    IMesaRepository Mesas { get; }
    IMeseroRepository Meseros { get; }
    IMenuRepository Menu { get; }
    IOrdenRepository Ordenes { get; }

    /// <summary>
    /// Persiste todos los cambios pendientes en una sola transacción.
    /// Lanza ConflictoConcurrenciaException si el RowVersion de alguna entidad ya cambió.
    /// </summary>
    Task GuardarCambiosAsync(CancellationToken ct = default);
}
