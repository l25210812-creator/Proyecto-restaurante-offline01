using Microsoft.EntityFrameworkCore;
using RestauranteApp.Domain.Entities;
using RestauranteApp.Domain.Interfaces.Repositories;

namespace RestauranteApp.Data.Repositories;

public class MenuRepository : IMenuRepository
{
    private readonly AppDbContext _db;

    public MenuRepository(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<CategoriaMenu>> ObtenerMenuCompletoAsync(CancellationToken ct = default) =>
        await _db.CategoriasMenu
            .Include(c => c.Items.Where(i => i.Disponible))
                .ThenInclude(i => i.Modificadores)
            .AsNoTracking()
            .OrderBy(c => c.Orden)
            .ToListAsync(ct);

    public Task<ItemMenu?> ObtenerItemPorIdAsync(int itemMenuId, CancellationToken ct = default) =>
        _db.ItemsMenu.Include(i => i.Modificadores).FirstOrDefaultAsync(i => i.Id == itemMenuId, ct);
}
