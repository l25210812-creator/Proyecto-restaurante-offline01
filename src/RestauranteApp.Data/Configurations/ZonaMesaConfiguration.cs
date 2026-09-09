using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestauranteApp.Domain.Entities;

namespace RestauranteApp.Data.Configurations;

public class ZonaConfiguration : IEntityTypeConfiguration<Zona>
{
    public void Configure(EntityTypeBuilder<Zona> builder)
    {
        builder.Property(z => z.Nombre).IsRequired().HasMaxLength(80);
        builder.HasMany(z => z.Mesas).WithOne(m => m.Zona).HasForeignKey(m => m.ZonaId);
        builder.Property(z => z.RowVersion).IsConcurrencyToken();
    }
}

public class MesaConfiguration : IEntityTypeConfiguration<Mesa>
{
    public void Configure(EntityTypeBuilder<Mesa> builder)
    {
        builder.Property(m => m.Nombre).IsRequired().HasMaxLength(40);
        builder.Property(m => m.RowVersion).IsConcurrencyToken();

        // Relación 1-a-1 opcional Mesa -> Orden activa, sin ciclo de borrado en cascada:
        // si se borra una orden, la mesa simplemente queda sin OrdenActivaId.
        builder.HasOne(m => m.OrdenActiva)
            .WithMany()
            .HasForeignKey(m => m.OrdenActivaId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
