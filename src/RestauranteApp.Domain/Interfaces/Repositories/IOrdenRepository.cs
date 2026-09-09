using RestauranteApp.Domain.Entities;

namespace RestauranteApp.Domain.Interfaces.Repositories;

public interface IOrdenRepository
{
    Task<Orden?> ObtenerAbiertaPorMesaAsync(int mesaId, CancellationToken ct = default);
    Task<Orden?> ObtenerConItemsAsync(int ordenId, CancellationToken ct = default);
    Task<Orden?> ObtenerPorItemOrdenIdAsync(int itemOrdenId, CancellationToken ct = default);
    Task<int> SiguienteFolioAsync(CancellationToken ct = default);
    Task AgregarAsync(Orden orden, CancellationToken ct = default);
    void MarcarModificada(Orden orden);
}
