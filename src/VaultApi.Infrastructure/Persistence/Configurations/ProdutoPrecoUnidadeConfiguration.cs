using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VaultApi.Domain.Entities;

namespace VaultApi.Infrastructure.Persistence.Configurations;

public class ProdutoPrecoUnidadeConfiguration : IEntityTypeConfiguration<ProdutoPrecoUnidade>
{
    public void Configure(EntityTypeBuilder<ProdutoPrecoUnidade> builder)
    {
        builder.HasKey(pu => pu.Id);
        builder.Property(pu => pu.TipoUnidade).HasConversion<string>().HasMaxLength(20);
        builder.HasIndex(pu => new { pu.ProdutoId, pu.TipoUnidade }).IsUnique();
    }
}
