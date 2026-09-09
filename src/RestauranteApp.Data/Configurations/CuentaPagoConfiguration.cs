using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestauranteApp.Domain.Entities;

namespace RestauranteApp.Data.Configurations;

public class CuentaConfiguration : IEntityTypeConfiguration<Cuenta>
{
    public void Configure(EntityTypeBuilder<Cuenta> builder)
    {
        builder.Property(c => c.Subtotal).HasColumnType("decimal(10,2)");
        builder.Property(c => c.Impuestos).HasColumnType("decimal(10,2)");
        builder.Property(c => c.Propina).HasColumnType("decimal(10,2)");
        builder.Property(c => c.Total).HasColumnType("decimal(10,2)");
        builder.Property(c => c.RowVersion).IsConcurrencyToken();

        builder.HasMany(c => c.Pagos)
            .WithOne(p => p.Cuenta)
            .HasForeignKey(p => p.CuentaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class PagoConfiguration : IEntityTypeConfiguration<Pago>
{
    public void Configure(EntityTypeBuilder<Pago> builder)
    {
        builder.Property(p => p.Monto).HasColumnType("decimal(10,2)");
        builder.Property(p => p.RowVersion).IsConcurrencyToken();
    }
}

public class RegistroImpresionConfiguration : IEntityTypeConfiguration<RegistroImpresion>
{
    public void Configure(EntityTypeBuilder<RegistroImpresion> builder)
    {
        builder.Property(r => r.NombreImpresora).IsRequired().HasMaxLength(80);
        builder.Property(r => r.RowVersion).IsConcurrencyToken();
    }
}

public class RestauranteConfiguration : IEntityTypeConfiguration<Restaurante>
{
    public void Configure(EntityTypeBuilder<Restaurante> builder)
    {
        builder.Property(r => r.Nombre).IsRequired().HasMaxLength(120);
        builder.Property(r => r.PorcentajeImpuesto).HasColumnType("decimal(5,4)");
        builder.Property(r => r.RowVersion).IsConcurrencyToken();
    }
}
