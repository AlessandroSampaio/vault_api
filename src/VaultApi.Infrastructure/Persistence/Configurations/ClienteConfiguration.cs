using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VaultApi.Domain.Entities;

namespace VaultApi.Infrastructure.Persistence.Configurations;

public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.RazaoSocial).IsRequired().HasMaxLength(200);
        builder.Property(c => c.NomeFantasia).HasMaxLength(200);
        builder.Property(c => c.Cnpj).IsRequired().HasMaxLength(20);
        builder.HasOne<Revenda>().WithMany().HasForeignKey(c => c.RevendaId).OnDelete(DeleteBehavior.Restrict);

        builder.Property(c => c.Cep).HasMaxLength(10);
        builder.Property(c => c.Logradouro).HasMaxLength(200);
        builder.Property(c => c.Numero).HasMaxLength(20);
        builder.Property(c => c.Complemento).HasMaxLength(100);
        builder.Property(c => c.Bairro).HasMaxLength(100);
        builder.Property(c => c.Cidade).HasMaxLength(100);
        builder.Property(c => c.Estado).HasMaxLength(2);

        builder.Property(c => c.Email).HasMaxLength(200);
        builder.Property(c => c.Telefone).HasMaxLength(20);
        builder.Property(c => c.Whatsapp).HasMaxLength(20);
        builder.Property(c => c.Responsavel).HasMaxLength(200);
    }
}
