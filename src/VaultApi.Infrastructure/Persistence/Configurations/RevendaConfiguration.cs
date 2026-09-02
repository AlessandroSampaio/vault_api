using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VaultApi.Domain.Entities;

namespace VaultApi.Infrastructure.Persistence.Configurations;

public class RevendaConfiguration : IEntityTypeConfiguration<Revenda>
{
    public void Configure(EntityTypeBuilder<Revenda> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Nome).IsRequired().HasMaxLength(200);
        builder.Property(r => r.Cnpj).IsRequired().HasMaxLength(20);
    }
}
