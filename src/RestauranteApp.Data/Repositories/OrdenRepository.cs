using Microsoft.EntityFrameworkCore;
using RestauranteApp.Domain.Entities;
using RestauranteApp.Domain.Interfaces.Repositories;
using RestauranteApp.Shared.Enums;

namespace RestauranteApp.Data.Repositories;

public class OrdenRepository : IOrdenRepository
{
    private readonly AppDbContext _db;

    public OrdenRepository(AppDbContext db) => _db = db;

    public Task<Orden?> ObtenerAbiertaPorMesaAsync(int mesaId, CancellationToken ct = default) =>
        _db.Ordenes
            .Include(o => o.Items)
            .Include(o => o.Comandas).ThenInclude(c => c.Items)
            .FirstOrDefaultAsync(o => o.MesaId == mesaId &&
                o.Estado != EstadoOrden.Cobrada && o.Estado != EstadoOrden.Cancelada, ct);

    public Task<Orden?> ObtenerConItemsAsync(int ordenId, CancellationToken ct = default) =>
        _db.Ordenes
            .Include(o => o.Items)
            .Include(o => o.Comandas).ThenInclude(c => c.Items)
            .Include(o => o.Cuenta)
            .FirstOrDefaultAsync(o => o.Id == ordenId, ct);

    public Task<Orden?> ObtenerPorItemOrdenIdAsync(int itemOrdenId, CancellationToken ct = default) =>
        _db.Ordenes
            .Include(o => o.Items)
            .Include(o => o.Mesa)
            .FirstOrDefaultAsync(o => o.Items.Any(i => i.Id == itemOrdenId), ct);

    public async Task<int> SiguienteFolioAsync(CancellationToken ct = default)
    {
        var max = await _db.Ordenes.Select(o => (int?)o.Folio).MaxAsync(ct);
        return (max ?? 0) + 1;
    }

    public async Task AgregarAsync(Orden orden, CancellationToken ct = default) =>
        await _db.Ordenes.AddAsync(orden, ct);

    public void MarcarModificada(Orden orden)
    {
        if (_db.Entry(orden).State == EntityState.Detached)
            _db.Ordenes.Attach(orden);
        _db.Entry(orden).State = EntityState.Modified;
    }
}
