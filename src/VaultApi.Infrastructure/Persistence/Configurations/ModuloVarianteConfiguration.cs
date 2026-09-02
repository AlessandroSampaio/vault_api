using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VaultApi.Domain.Entities;

namespace VaultApi.Infrastructure.Persistence.Configurations;

public class ModuloVarianteConfiguration : IEntityTypeConfiguration<ModuloVariante>
{
    public void Configure(EntityTypeBuilder<ModuloVariante> builder)
    {
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Nome).IsRequired().HasMaxLength(200);
        builder.Property(v => v.TipoUnidadeAplicavel).HasConversion<string>().HasMaxLength(20);
    }
}
