using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestauranteApp.Domain.Entities;

namespace RestauranteApp.Data.Configurations;

public class OrdenConfiguration : IEntityTypeConfiguration<Orden>
{
    public void Configure(EntityTypeBuilder<Orden> builder)
    {
        builder.HasIndex(o => o.Folio).IsUnique();

        // Columna clave del bloqueo optimista: EF Core incluye RowVersion en el WHERE
        // del UPDATE/DELETE; si no coincide con lo que hay en BD, lanza
        // DbUpdateConcurrencyException (ver UnitOfWork.GuardarCambiosAsync).
        builder.Property(o => o.RowVersion).IsConcurrencyToken();

        builder.HasOne(o => o.Mesa)
            .WithMany()
            .HasForeignKey(o => o.MesaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.Mesero)
            .WithMany()
            .HasForeignKey(o => o.MeseroId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(o => o.Items)
            .WithOne(i => i.Orden)
            .HasForeignKey(i => i.OrdenId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(o => o.Comandas)
            .WithOne(c => c.Orden)
            .HasForeignKey(c => c.OrdenId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(o => o.Cuenta)
            .WithOne(c => c.Orden)
            .HasForeignKey<Cuenta>(c => c.OrdenId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ItemOrdenConfiguration : IEntityTypeConfiguration<ItemOrden>
{
    public void Configure(EntityTypeBuilder<ItemOrden> builder)
    {
        builder.Property(i => i.NombreItem).IsRequired().HasMaxLength(120);
        builder.Property(i => i.PrecioUnitario).HasColumnType("decimal(10,2)");
        builder.Property(i => i.RowVersion).IsConcurrencyToken();

        builder.HasOne(i => i.ItemMenu)
            .WithMany()
            .HasForeignKey(i => i.ItemMenuId)
            .OnDelete(DeleteBehavior.Restrict);

        // La cascada de borrado de ítems ya la maneja Orden->Items; esta FK hacia Comanda
        // se deja en Restrict para evitar múltiples rutas de cascada sobre la misma tabla.
        builder.HasOne(i => i.Comanda)
            .WithMany(c => c.Items)
            .HasForeignKey(i => i.ComandaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ComandaConfiguration : IEntityTypeConfiguration<Comanda>
{
    public void Configure(EntityTypeBuilder<Comanda> builder)
    {
        builder.Property(c => c.RowVersion).IsConcurrencyToken();
    }
}
