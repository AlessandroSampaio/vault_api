using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VaultApi.Domain.Entities;

namespace VaultApi.Infrastructure.Persistence.Configurations;

public class ContratoItemUnidadeConfiguration : IEntityTypeConfiguration<ContratoItemUnidade>
{
    public void Configure(EntityTypeBuilder<ContratoItemUnidade> builder)
    {
        builder.HasKey(u => u.Id);
        builder.Property(u => u.TipoUnidade).HasConversion<string>().HasMaxLength(20);
        builder.HasIndex(u => new { u.ContratoItemId, u.TipoUnidade }).IsUnique();
    }
}
