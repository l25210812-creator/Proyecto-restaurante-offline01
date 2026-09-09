using Microsoft.EntityFrameworkCore;
using RestauranteApp.Data.Repositories;
using RestauranteApp.Domain.Exceptions;
using RestauranteApp.Domain.Interfaces;
using RestauranteApp.Domain.Interfaces.Repositories;

namespace RestauranteApp.Data;

/// <summary>
/// Une todos los repositorios sobre un único AppDbContext (vida "scoped" por request/operación),
/// de forma que GuardarCambiosAsync persiste todo en una sola transacción implícita de EF Core.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _db;

    public UnitOfWork(AppDbContext db)
    {
        _db = db;
        Mesas = new MesaRepository(db);
        Meseros = new MeseroRepository(db);
        Menu = new MenuRepository(db);
        Ordenes = new OrdenRepository(db);
    }

    public IMesaRepository Mesas { get; }
    public IMeseroRepository Meseros { get; }
    public IMenuRepository Menu { get; }
    public IOrdenRepository Ordenes { get; }

    public async Task GuardarCambiosAsync(CancellationToken ct = default)
    {
        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // Traduce la excepción específica de EF Core a la excepción de dominio que
            // OrdenService/CuentaService ya saben interpretar (recargar y reintentar).
            throw new ConflictoConcurrenciaException(
                "Otra terminal ya modificó esta información. Vuelve a cargarla e intenta de nuevo.", ex);
        }
    }
}
