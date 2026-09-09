using Microsoft.EntityFrameworkCore;
using RestauranteApp.Domain.Entities;
using RestauranteApp.Domain.Interfaces.Repositories;

namespace RestauranteApp.Data.Repositories;

public class MesaRepository : IMesaRepository
{
    private readonly AppDbContext _db;

    public MesaRepository(AppDbContext db) => _db = db;

    public Task<Mesa?> ObtenerPorIdAsync(int id, CancellationToken ct = default) =>
        _db.Mesas.Include(m => m.Zona).FirstOrDefaultAsync(m => m.Id == id, ct);

    public async Task<IReadOnlyList<Mesa>> ObtenerTodasConZonaAsync(CancellationToken ct = default) =>
        await _db.Mesas.Include(m => m.Zona).AsNoTracking().OrderBy(m => m.ZonaId).ThenBy(m => m.Nombre).ToListAsync(ct);

    public Task ActualizarAsync(Mesa mesa, CancellationToken ct = default)
    {
        if (_db.Entry(mesa).State == EntityState.Detached)
            _db.Mesas.Attach(mesa);
        _db.Entry(mesa).State = EntityState.Modified;
        return Task.CompletedTask;
    }
}
