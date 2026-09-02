using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VaultApi.Domain.Entities;

namespace VaultApi.Infrastructure.Persistence.Configurations;

public class RevendaConfiguration : IEntityTypeConfiguration<Revenda>
{
    public void Configure(EntityTypeBuilder<Revenda> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.RazaoSocial).IsRequired().HasMaxLength(200);
        builder.Property(r => r.NomeFantasia).HasMaxLength(200);
        builder.Property(r => r.Cnpj).IsRequired().HasMaxLength(20);

        builder.Property(r => r.Cep).HasMaxLength(10);
        builder.Property(r => r.Logradouro).HasMaxLength(200);
        builder.Property(r => r.Numero).HasMaxLength(20);
        builder.Property(r => r.Complemento).HasMaxLength(100);
        builder.Property(r => r.Bairro).HasMaxLength(100);
        builder.Property(r => r.Cidade).HasMaxLength(100);
        builder.Property(r => r.Estado).HasMaxLength(2);

        builder.Property(r => r.Email).HasMaxLength(200);
        builder.Property(r => r.Telefone).HasMaxLength(20);
        builder.Property(r => r.Whatsapp).HasMaxLength(20);
        builder.Property(r => r.Responsavel).HasMaxLength(200);
    }
}
