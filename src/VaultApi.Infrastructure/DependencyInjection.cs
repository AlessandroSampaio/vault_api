using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VaultApi.Infrastructure.Persistence;

namespace VaultApi.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options => options
            .UseNpgsql(configuration.GetConnectionString("Default"))
            .UseSnakeCaseNamingConvention());

        services.AddIdentityCore<Domain.Entities.Usuario>(options =>
            {
                options.Password.RequireNonAlphanumeric = false;
                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<Microsoft.AspNetCore.Identity.IdentityRole<Guid>>()
            .AddSignInManager()
            .AddEntityFrameworkStores<Persistence.AppDbContext>();

        services.AddScoped<Application.Abstractions.ITokenService, Auth.TokenService>();

        return services;
    }
}
