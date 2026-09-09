using RestauranteApp.Domain.Entities;
using RestauranteApp.Domain.Interfaces.Repositories;
using RestauranteApp.Shared.Enums;

namespace RestauranteApp.Tests.Fakes;

public class FakeMesaRepository : IMesaRepository
{
    private readonly FakeDatabaseStore _store;

    public FakeMesaRepository(FakeDatabaseStore store) => _store = store;

    public Task<Mesa?> ObtenerPorIdAsync(int id, CancellationToken ct = default) =>
        Task.FromResult(_store.Mesas.TryGetValue(id, out var mesa) ? mesa : null);

    public Task<IReadOnlyList<Mesa>> ObtenerTodasConZonaAsync(CancellationToken ct = default) =>
        Task.FromResult((IReadOnlyList<Mesa>)_store.Mesas.Values.ToList());

    public Task ActualizarAsync(Mesa mesa, CancellationToken ct = default)
    {
        _store.Mesas[mesa.Id] = mesa; // en las pruebas trabajamos siempre con la misma instancia
        return Task.CompletedTask;
    }
}

public class FakeMeseroRepository : IMeseroRepository
{
    private readonly FakeDatabaseStore _store;

    public FakeMeseroRepository(FakeDatabaseStore store) => _store = store;

    public Task<Mesero?> ObtenerPorNumeroAsync(int numeroMesero, CancellationToken ct = default) =>
        Task.FromResult(_store.Meseros.Values.FirstOrDefault(m => m.NumeroMesero == numeroMesero));

    public Task<Mesero?> ObtenerPorIdAsync(int id, CancellationToken ct = default) =>
        Task.FromResult(_store.Meseros.TryGetValue(id, out var m) ? m : null);
}

public class FakeMenuRepository : IMenuRepository
{
    private readonly FakeDatabaseStore _store;

    public FakeMenuRepository(FakeDatabaseStore store) => _store = store;

    public Task<IReadOnlyList<CategoriaMenu>> ObtenerMenuCompletoAsync(CancellationToken ct = default) =>
        Task.FromResult((IReadOnlyList<CategoriaMenu>)new List<CategoriaMenu>());

    public Task<ItemMenu?> ObtenerItemPorIdAsync(int itemMenuId, CancellationToken ct = default) =>
        Task.FromResult(_store.ItemsMenu.TryGetValue(itemMenuId, out var item) ? item : null);
}

/// <summary>
/// Repositorio fake de órdenes. Cada "Obtener" devuelve un CLON, imitando que cada
/// operación (cada terminal) trabaja sobre su propia copia hasta que GuardarCambiosAsync
/// la reconcilia contra el "store" -- justo el escenario que dispara el bloqueo optimista.
/// </summary>
public class FakeOrdenRepository : IOrdenRepository
{
    private readonly FakeDatabaseStore _store;

    internal readonly List<Orden> Nuevas = new();
    internal readonly List<Orden> Modificadas = new();

    public FakeOrdenRepository(FakeDatabaseStore store) => _store = store;

    public Task<Orden?> ObtenerAbiertaPorMesaAsync(int mesaId, CancellationToken ct = default)
    {
        var orden = _store.Ordenes.Values.FirstOrDefault(o =>
            o.MesaId == mesaId && o.Estado != EstadoOrden.Cobrada && o.Estado != EstadoOrden.Cancelada);
        return Task.FromResult(orden is null ? null : FakeDatabaseStore.Clonar(orden));
    }

    public Task<Orden?> ObtenerConItemsAsync(int ordenId, CancellationToken ct = default) =>
        Task.FromResult(_store.Ordenes.TryGetValue(ordenId, out var orden) ? FakeDatabaseStore.Clonar(orden) : null);

    public Task<Orden?> ObtenerPorItemOrdenIdAsync(int itemOrdenId, CancellationToken ct = default)
    {
        var orden = _store.Ordenes.Values.FirstOrDefault(o => o.Items.Any(i => i.Id == itemOrdenId));
        return Task.FromResult(orden is null ? null : FakeDatabaseStore.Clonar(orden));
    }

    public Task<int> SiguienteFolioAsync(CancellationToken ct = default)
    {
        var max = _store.Ordenes.Values.Select(o => (int?)o.Folio).Max();
        return Task.FromResult((max ?? 0) + 1);
    }

    public Task AgregarAsync(Orden orden, CancellationToken ct = default)
    {
        Nuevas.Add(orden);
        return Task.CompletedTask;
    }

    public void MarcarModificada(Orden orden) => Modificadas.Add(orden);
}
