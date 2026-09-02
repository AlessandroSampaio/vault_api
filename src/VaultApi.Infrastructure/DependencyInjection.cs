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

        return services;
    }
}
