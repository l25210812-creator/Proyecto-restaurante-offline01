using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestauranteApp.Domain.Entities;

namespace RestauranteApp.Data.Configurations;

public class CategoriaMenuConfiguration : IEntityTypeConfiguration<CategoriaMenu>
{
    public void Configure(EntityTypeBuilder<CategoriaMenu> builder)
    {
        builder.Property(c => c.Nombre).IsRequired().HasMaxLength(60);
        builder.HasMany(c => c.Items).WithOne(i => i.CategoriaMenu).HasForeignKey(i => i.CategoriaMenuId);
        builder.Property(c => c.RowVersion).IsConcurrencyToken();
    }
}

public class ItemMenuConfiguration : IEntityTypeConfiguration<ItemMenu>
{
    public void Configure(EntityTypeBuilder<ItemMenu> builder)
    {
        builder.Property(i => i.Nombre).IsRequired().HasMaxLength(120);
        builder.Property(i => i.Precio).HasColumnType("decimal(10,2)");
        builder.HasMany(i => i.Modificadores).WithOne(m => m.ItemMenu).HasForeignKey(m => m.ItemMenuId);
        builder.Property(i => i.RowVersion).IsConcurrencyToken();
    }
}

public class ModificadorItemConfiguration : IEntityTypeConfiguration<ModificadorItem>
{
    public void Configure(EntityTypeBuilder<ModificadorItem> builder)
    {
        builder.Property(m => m.Nombre).IsRequired().HasMaxLength(60);
        builder.Property(m => m.PrecioExtra).HasColumnType("decimal(10,2)");
        builder.Property(m => m.RowVersion).IsConcurrencyToken();
    }
}
