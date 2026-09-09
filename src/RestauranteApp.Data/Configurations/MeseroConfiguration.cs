using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestauranteApp.Domain.Entities;

namespace RestauranteApp.Data.Configurations;

public class MeseroConfiguration : IEntityTypeConfiguration<Mesero>
{
    public void Configure(EntityTypeBuilder<Mesero> builder)
    {
        builder.Property(m => m.Nombre).IsRequired().HasMaxLength(80);
        builder.HasIndex(m => m.NumeroMesero).IsUnique();
        builder.Property(m => m.PinHash).IsRequired();
        builder.Property(m => m.RowVersion).IsConcurrencyToken();
    }
}
