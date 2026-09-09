using RestauranteApp.Domain.Entities;
using RestauranteApp.Domain.Exceptions;
using RestauranteApp.Domain.Interfaces;
using RestauranteApp.Domain.Interfaces.Repositories;

namespace RestauranteApp.Tests.Fakes;

public class FakeUnitOfWork : IUnitOfWork
{
    private readonly FakeDatabaseStore _store;
    private readonly FakeOrdenRepository _ordenRepo;

    public FakeUnitOfWork(FakeDatabaseStore store)
    {
        _store = store;
        Mesas = new FakeMesaRepository(store);
        Meseros = new FakeMeseroRepository(store);
        Menu = new FakeMenuRepository(store);
        _ordenRepo = new FakeOrdenRepository(store);
        Ordenes = _ordenRepo;
    }

    public IMesaRepository Mesas { get; }
    public IMeseroRepository Meseros { get; }
    public IMenuRepository Menu { get; }
    public IOrdenRepository Ordenes { get; }

    /// <summary>
    /// Reproduce lo esencial de AppDbContext.SaveChangesAsync: genera un RowVersion nuevo
    /// por cada entidad nueva/modificada y, para las modificadas, exige que el RowVersion
    /// que trae la entidad coincida con el que hay en el "store" -- si no coincide, alguien
    /// más ya guardó cambios primero (bloqueo optimista), igual que DbUpdateConcurrencyException.
    /// </summary>
    public Task GuardarCambiosAsync(CancellationToken ct = default)
    {
        foreach (var nueva in _ordenRepo.Nuevas)
        {
            nueva.Id = _store.SiguienteOrdenId();
            foreach (var item in nueva.Items)
                item.Id = _store.SiguienteItemOrdenId();
            foreach (var comanda in nueva.Comandas)
                comanda.Id = _store.SiguienteComandaId();

            nueva.RowVersion = FakeDatabaseStore.NuevoRowVersion();
            _store.Ordenes[nueva.Id] = FakeDatabaseStore.Clonar(nueva);
        }

        foreach (var modificada in _ordenRepo.Modificadas)
        {
            var actual = _store.Ordenes[modificada.Id];
            if (!actual.RowVersion.SequenceEqual(modificada.RowVersion))
            {
                throw new ConflictoConcurrenciaException(
                    $"La orden {modificada.Id} fue modificada por otra terminal (simulado en prueba).");
            }

            foreach (var item in modificada.Items.Where(i => i.Id == 0))
                item.Id = _store.SiguienteItemOrdenId();
            foreach (var comanda in modificada.Comandas.Where(c => c.Id == 0))
                comanda.Id = _store.SiguienteComandaId();

            modificada.RowVersion = FakeDatabaseStore.NuevoRowVersion();
            _store.Ordenes[modificada.Id] = FakeDatabaseStore.Clonar(modificada);
        }

        _ordenRepo.Nuevas.Clear();
        _ordenRepo.Modificadas.Clear();
        return Task.CompletedTask;
    }
}
