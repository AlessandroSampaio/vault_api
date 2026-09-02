using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VaultApi.Domain.Entities;

namespace VaultApi.Infrastructure.Persistence.Configurations;

public class ProdutoConfiguration : IEntityTypeConfiguration<Produto>
{
    public void Configure(EntityTypeBuilder<Produto> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Nome).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Descricao).HasMaxLength(1000);
        builder.HasMany(p => p.PrecosPorUnidade).WithOne().HasForeignKey(pu => pu.ProdutoId);
        builder.HasMany(p => p.Modulos).WithOne().HasForeignKey(m => m.ProdutoId);
    }
}
