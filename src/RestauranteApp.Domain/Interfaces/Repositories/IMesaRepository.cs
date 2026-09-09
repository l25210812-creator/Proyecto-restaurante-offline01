using RestauranteApp.Domain.Entities;

namespace RestauranteApp.Domain.Interfaces.Repositories;

public interface IMesaRepository
{
    Task<Mesa?> ObtenerPorIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<Mesa>> ObtenerTodasConZonaAsync(CancellationToken ct = default);
    Task ActualizarAsync(Mesa mesa, CancellationToken ct = default);
}
