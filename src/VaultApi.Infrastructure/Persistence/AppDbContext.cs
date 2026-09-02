using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VaultApi.Domain.Entities;

namespace VaultApi.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<Usuario, IdentityRole<Guid>, Guid>(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // IdentityDbContext hardcodes .ToTable("AspNetUsers") etc. and named indexes
        // via explicit fluent calls, which EFCore.NamingConventions never touches
        // (it only renames identifiers left unset). Re-map them here to snake_case.
        modelBuilder.Entity<Usuario>().ToTable("usuarios");
        modelBuilder.Entity<IdentityRole<Guid>>().ToTable("roles");
        modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable("usuario_claims");
        modelBuilder.Entity<IdentityUserRole<Guid>>().ToTable("usuario_roles");
        modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable("usuario_logins");
        modelBuilder.Entity<IdentityRoleClaim<Guid>>().ToTable("role_claims");
        modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable("usuario_tokens");

        modelBuilder.Entity<Usuario>().HasIndex(u => u.NormalizedUserName).HasDatabaseName("ix_usuarios_normalized_user_name");
        modelBuilder.Entity<Usuario>().HasIndex(u => u.NormalizedEmail).HasDatabaseName("ix_usuarios_normalized_email");
        modelBuilder.Entity<IdentityRole<Guid>>().HasIndex(r => r.NormalizedName).HasDatabaseName("ix_roles_normalized_name");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
