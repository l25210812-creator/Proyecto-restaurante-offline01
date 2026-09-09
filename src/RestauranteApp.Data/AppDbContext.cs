using Microsoft.EntityFrameworkCore;
using RestauranteApp.Domain.Common;
using RestauranteApp.Domain.Entities;

namespace RestauranteApp.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Restaurante> Restaurantes => Set<Restaurante>();
    public DbSet<Zona> Zonas => Set<Zona>();
    public DbSet<Mesa> Mesas => Set<Mesa>();
    public DbSet<Mesero> Meseros => Set<Mesero>();
    public DbSet<CategoriaMenu> CategoriasMenu => Set<CategoriaMenu>();
    public DbSet<ItemMenu> ItemsMenu => Set<ItemMenu>();
    public DbSet<ModificadorItem> ModificadoresItem => Set<ModificadorItem>();
    public DbSet<Orden> Ordenes => Set<Orden>();
    public DbSet<ItemOrden> ItemsOrden => Set<ItemOrden>();
    public DbSet<Comanda> Comandas => Set<Comanda>();
    public DbSet<Cuenta> Cuentas => Set<Cuenta>();
    public DbSet<Pago> Pagos => Set<Pago>();
    public DbSet<RegistroImpresion> RegistrosImpresion => Set<RegistroImpresion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    /// <summary>
    /// Genera un RowVersion nuevo para cada entidad agregada o modificada antes de guardar.
    /// Esto implementa bloqueo optimista de forma portable (funciona igual en SQLite que en
    /// SQL Server), a diferencia de IsRowVersion(), que solo tiene soporte nativo en SQL Server.
    /// EF Core compara el valor original contra lo que hay en BD al hacer UPDATE/DELETE y,
    /// si no coincide (alguien más ya guardó cambios), lanza DbUpdateConcurrencyException.
    /// </summary>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ActualizarRowVersions();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        ActualizarRowVersions();
        return base.SaveChanges();
    }

    private void ActualizarRowVersions()
    {
        foreach (var entry in ChangeTracker.Entries<EntidadBase>())
        {
            if (entry.State is EntityState.Added or EntityState.Modified)
            {
                entry.Property(nameof(EntidadBase.RowVersion)).CurrentValue = Guid.NewGuid().ToByteArray();
            }
        }
    }
}
