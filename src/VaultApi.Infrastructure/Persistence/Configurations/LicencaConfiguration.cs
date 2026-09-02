using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VaultApi.Domain.Entities;

namespace VaultApi.Infrastructure.Persistence.Configurations;

public class LicencaConfiguration : IEntityTypeConfiguration<Licenca>
{
    public void Configure(EntityTypeBuilder<Licenca> builder)
    {
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Serial).IsRequired();
        builder.Property(l => l.Algoritmo).IsRequired().HasMaxLength(50);
        builder.Property(l => l.Status).HasConversion<string>().HasMaxLength(20);
    }
}
