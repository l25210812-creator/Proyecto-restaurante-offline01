using Microsoft.EntityFrameworkCore;
using RestauranteApp.Domain.Entities;
using RestauranteApp.Domain.Interfaces.Repositories;

namespace RestauranteApp.Data.Repositories;

public class MeseroRepository : IMeseroRepository
{
    private readonly AppDbContext _db;

    public MeseroRepository(AppDbContext db) => _db = db;

    public Task<Mesero?> ObtenerPorNumeroAsync(int numeroMesero, CancellationToken ct = default) =>
        _db.Meseros.FirstOrDefaultAsync(m => m.NumeroMesero == numeroMesero, ct);

    public Task<Mesero?> ObtenerPorIdAsync(int id, CancellationToken ct = default) =>
        _db.Meseros.FirstOrDefaultAsync(m => m.Id == id, ct);
}
