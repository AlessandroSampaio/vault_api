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

        services.AddScoped<Domain.Repositories.IRevendaRepository, Repositories.RevendaRepository>();
        services.AddScoped<Application.Revendas.RevendaService>();

        services.AddScoped<Domain.Repositories.IClienteRepository, Repositories.ClienteRepository>();
        services.AddScoped<Application.Clientes.ClienteService>();

        services.AddScoped<Application.Abstractions.IScopeFilter, Application.Scope.ScopeFilter>();

        services.AddScoped<Domain.Repositories.IProdutoRepository, Repositories.ProdutoRepository>();
        services.AddScoped<Domain.Repositories.IHistoricoPrecoCatalogoRepository, Repositories.HistoricoPrecoCatalogoRepository>();
        services.AddScoped<Application.Catalogo.CatalogoService>();

        services.AddScoped<Domain.Repositories.ILicencaRepository, Repositories.LicencaRepository>();
        services.AddScoped<Application.Licencas.LicencaService>();

        services.AddScoped<Domain.Repositories.IContratoRepository, Repositories.ContratoRepository>();
        services.AddScoped<Application.Contratos.ContratoService>();

        return services;
    }
}
