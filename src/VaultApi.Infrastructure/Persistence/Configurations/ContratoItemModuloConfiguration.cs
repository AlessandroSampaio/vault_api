using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VaultApi.Domain.Entities;

namespace VaultApi.Infrastructure.Persistence.Configurations;

public class ContratoItemModuloConfiguration : IEntityTypeConfiguration<ContratoItemModulo>
{
    public void Configure(EntityTypeBuilder<ContratoItemModulo> builder)
    {
        builder.HasKey(m => m.Id);
    }
}
