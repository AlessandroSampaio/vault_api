using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VaultApi.Domain.Entities;

namespace VaultApi.Infrastructure.Persistence.Configurations;

public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Nome).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Cnpj).IsRequired().HasMaxLength(20);
        builder.HasOne<Revenda>().WithMany().HasForeignKey(c => c.RevendaId).OnDelete(DeleteBehavior.Restrict);
    }
}
