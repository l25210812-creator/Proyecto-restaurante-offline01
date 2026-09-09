using RestauranteApp.Domain.Entities;

namespace RestauranteApp.Domain.Interfaces.Repositories;

public interface IMeseroRepository
{
    Task<Mesero?> ObtenerPorNumeroAsync(int numeroMesero, CancellationToken ct = default);
    Task<Mesero?> ObtenerPorIdAsync(int id, CancellationToken ct = default);
}
