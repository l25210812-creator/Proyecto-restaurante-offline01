using RestauranteApp.Domain.Entities;

namespace RestauranteApp.Domain.Interfaces.Repositories;

public interface IMenuRepository
{
    Task<IReadOnlyList<CategoriaMenu>> ObtenerMenuCompletoAsync(CancellationToken ct = default);
    Task<ItemMenu?> ObtenerItemPorIdAsync(int itemMenuId, CancellationToken ct = default);
}
