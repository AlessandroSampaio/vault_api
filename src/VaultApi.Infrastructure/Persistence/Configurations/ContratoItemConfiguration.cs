using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VaultApi.Domain.Entities;

namespace VaultApi.Infrastructure.Persistence.Configurations;

public class ContratoItemConfiguration : IEntityTypeConfiguration<ContratoItem>
{
    public void Configure(EntityTypeBuilder<ContratoItem> builder)
    {
        builder.HasKey(i => i.Id);
        builder.Property(i => i.TipoDesconto).HasConversion<string>().HasMaxLength(20);
        builder.HasMany(i => i.Unidades).WithOne().HasForeignKey(u => u.ContratoItemId);
        builder.HasMany(i => i.Modulos).WithOne().HasForeignKey(m => m.ContratoItemId);
    }
}
