using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VaultApi.Domain.Entities;

namespace VaultApi.Infrastructure.Persistence.Configurations;

public class HistoricoPrecoCatalogoConfiguration : IEntityTypeConfiguration<HistoricoPrecoCatalogo>
{
    public void Configure(EntityTypeBuilder<HistoricoPrecoCatalogo> builder)
    {
        builder.HasKey(h => h.Id);
        builder.Property(h => h.EntidadeTipo).HasConversion<string>().HasMaxLength(30);
        builder.Property(h => h.TipoValor).HasConversion<string>().HasMaxLength(30);
    }
}
