# VaultApi Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Bootstrap VaultApi from the current `dotnet new webapi` template into a Clean Architecture .NET 10 API (Domain/Application/Infrastructure/Api + Tests) backed by PostgreSQL/EF Core, implementing the usuário/cliente/revenda/produto/contrato licensing domain described in the design spec.

**Architecture:** 4 projects (Domain, Application, Infrastructure, Api) + 1 test project, Clean Architecture with no CQRS/MediatR. Domain holds entities/enums/repository interfaces only. Application holds services, DTOs, the pure `PricingResolver`, and `IScopeFilter`. Infrastructure holds EF Core + Npgsql + Identity + repository implementations. Api holds controllers, JWT issuing, and authorization policies.

**Tech Stack:** .NET 10, ASP.NET Core Identity, EF Core 10 + Npgsql.EntityFrameworkCore.PostgreSQL, EFCore.NamingConventions (snake_case), Microsoft.AspNetCore.Authentication.JwtBearer, xUnit, FluentAssertions, Testcontainers.PostgreSql, Microsoft.AspNetCore.Mvc.Testing. (Request validation is out of scope for this plan — no validation rules were specified beyond entity shape; add FluentValidation in a follow-up plan if/when concrete rules are defined.)

**Spec:** `docs/superpowers/specs/2026-08-29-vault-api-design.md`

## Global Constraints

- Target framework: `net10.0` for every project.
- Table/column naming: snake_case (via `EFCore.NamingConventions`), configured once in `AppDbContext` setup.
- Only Admin (`Nivel.Admin`) may write Contrato, Revenda, Usuario, Produto/Modulo/ModuloVariante. Revenda and Usuario have read-only access, scoped to their own `RevendaId`, and never see negotiated contract prices (see `IScopeFilter` in Task 8 and DTO split in Task 13).
- Migrations live in `src/VaultApi.Infrastructure/Migrations`, generated via `dotnet ef migrations add`. Auto-apply only in Development (`Program.cs` checks `IHostEnvironment.IsDevelopment()`); never auto-apply in production.
- `PricingResolver` and `IScopeFilter`/`ScopeResult` logic must be pure (no EF/HTTP dependency) and unit-tested without a database.
- Integration tests use Testcontainers with a real Postgres — never the EF in-memory provider — via the shared fixture built in Task 2.
- Every task ends green: `dotnet build` succeeds and the task's own tests pass before moving to the next task.

---

## File Structure

```
VaultApi.slnx
docker-compose.yml
README.md
src/
  VaultApi.Domain/
    VaultApi.Domain.csproj
    Enums/Nivel.cs
    Enums/TipoUnidade.cs
    Enums/TipoDesconto.cs
    Enums/StatusLicenca.cs
    Enums/EntidadeTipoCatalogo.cs
    Enums/TipoValorCatalogo.cs
    Entities/Usuario.cs
    Entities/Revenda.cs
    Entities/Cliente.cs
    Entities/Produto.cs
    Entities/ProdutoPrecoUnidade.cs
    Entities/Modulo.cs
    Entities/ModuloVariante.cs
    Entities/Contrato.cs
    Entities/ContratoItem.cs
    Entities/ContratoItemUnidade.cs
    Entities/ContratoItemModulo.cs
    Entities/Licenca.cs
    Entities/HistoricoPrecoCatalogo.cs
    Repositories/IRevendaRepository.cs
    Repositories/IClienteRepository.cs
    Repositories/IProdutoRepository.cs
    Repositories/IContratoRepository.cs
    Repositories/ILicencaRepository.cs
  VaultApi.Application/
    VaultApi.Application.csproj
    Abstractions/ICurrentUser.cs
    Abstractions/ITokenService.cs
    Abstractions/IScopeFilter.cs
    Abstractions/ScopeResult.cs
    Pricing/PricingResolver.cs
    Pricing/ItemPricingInput.cs
    Pricing/ItemPricingResult.cs
    Revendas/RevendaService.cs
    Revendas/RevendaDtos.cs
    Clientes/ClienteService.cs
    Clientes/ClienteDtos.cs
    Catalogo/CatalogoService.cs
    Catalogo/CatalogoDtos.cs
    Contratos/ContratoService.cs
    Contratos/ContratoDtos.cs
    Licencas/LicencaService.cs
  VaultApi.Infrastructure/
    VaultApi.Infrastructure.csproj
    Persistence/AppDbContext.cs
    Persistence/Configurations/*.cs
    Migrations/ (generated)
    Repositories/RevendaRepository.cs
    Repositories/ClienteRepository.cs
    Repositories/ProdutoRepository.cs
    Repositories/ContratoRepository.cs
    Repositories/LicencaRepository.cs
    Auth/TokenService.cs
    DependencyInjection.cs
  VaultApi.Api/
    VaultApi.Api.csproj
    Program.cs
    Auth/CurrentUser.cs
    Auth/PolicyNames.cs
    Controllers/AuthController.cs
    Controllers/RevendasController.cs
    Controllers/ClientesController.cs
    Controllers/CatalogoController.cs
    Controllers/ContratosController.cs
    appsettings.json
    appsettings.Development.json
tests/
  VaultApi.Tests/
    VaultApi.Tests.csproj
    Fixtures/PostgresFixture.cs
    Fixtures/ApiFactory.cs
    Pricing/PricingResolverTests.cs
    Scope/ScopeFilterTests.cs
    Auth/AuthEndpointTests.cs
    Revendas/RevendaEndpointTests.cs
    Clientes/ClienteEndpointTests.cs
    Catalogo/CatalogoEndpointTests.cs
    Catalogo/HistoricoPrecoTests.cs
    Contratos/ContratoEndpointTests.cs
    Licencas/LicencaServiceTests.cs
```

---

## Task 1: Solution Scaffolding

**Files:**
- Delete: `Api/` (entire old template directory)
- Create: `src/VaultApi.Domain/VaultApi.Domain.csproj`
- Create: `src/VaultApi.Application/VaultApi.Application.csproj`
- Create: `src/VaultApi.Infrastructure/VaultApi.Infrastructure.csproj`
- Create: `src/VaultApi.Api/VaultApi.Api.csproj`, `src/VaultApi.Api/Program.cs`, `src/VaultApi.Api/appsettings.json`, `src/VaultApi.Api/appsettings.Development.json`
- Create: `tests/VaultApi.Tests/VaultApi.Tests.csproj`
- Modify: `VaultApi.slnx`

**Interfaces:**
- Produces: solution with 5 projects, referenced as `VaultApi.Domain`, `VaultApi.Application`, `VaultApi.Infrastructure`, `VaultApi.Api`, `VaultApi.Tests` — later tasks add files into these projects.

- [ ] **Step 1: Remove the old template**

```bash
git rm -r Api
```

- [ ] **Step 2: Scaffold the 5 projects**

```bash
mkdir -p src tests
dotnet new classlib -n VaultApi.Domain -o src/VaultApi.Domain --framework net10.0
dotnet new classlib -n VaultApi.Application -o src/VaultApi.Application --framework net10.0
dotnet new classlib -n VaultApi.Infrastructure -o src/VaultApi.Infrastructure --framework net10.0
dotnet new webapi -n VaultApi.Api -o src/VaultApi.Api --framework net10.0 -controllers
dotnet new xunit -n VaultApi.Tests -o tests/VaultApi.Tests --framework net10.0

rm src/VaultApi.Domain/Class1.cs src/VaultApi.Application/Class1.cs src/VaultApi.Infrastructure/Class1.cs
rm -f src/VaultApi.Api/WeatherForecast.cs src/VaultApi.Api/Controllers/WeatherForecastController.cs
rm tests/VaultApi.Tests/UnitTest1.cs
```

- [ ] **Step 3: Wire project references**

```bash
dotnet add src/VaultApi.Application reference src/VaultApi.Domain
dotnet add src/VaultApi.Infrastructure reference src/VaultApi.Application
dotnet add src/VaultApi.Api reference src/VaultApi.Application src/VaultApi.Infrastructure
dotnet add tests/VaultApi.Tests reference src/VaultApi.Domain src/VaultApi.Application src/VaultApi.Infrastructure src/VaultApi.Api
```

- [ ] **Step 4: Rewrite the solution file**

Replace the full contents of `VaultApi.slnx`:

```xml
<Solution>
  <Project Path="src/VaultApi.Domain/VaultApi.Domain.csproj" />
  <Project Path="src/VaultApi.Application/VaultApi.Application.csproj" />
  <Project Path="src/VaultApi.Infrastructure/VaultApi.Infrastructure.csproj" />
  <Project Path="src/VaultApi.Api/VaultApi.Api.csproj" />
  <Project Path="tests/VaultApi.Tests/VaultApi.Tests.csproj" />
</Solution>
```

- [ ] **Step 5: Verify the solution builds**

Run: `dotnet build VaultApi.slnx`
Expected: `Build succeeded.` with 0 errors.

- [ ] **Step 6: Commit**

```bash
git add -A
git commit -m "chore: scaffold Clean Architecture project structure"
```

---

## Task 2: PostgreSQL Infrastructure + EF Core Shell

**Files:**
- Create: `docker-compose.yml`
- Create: `src/VaultApi.Infrastructure/Persistence/AppDbContext.cs`
- Create: `src/VaultApi.Infrastructure/DependencyInjection.cs`
- Modify: `src/VaultApi.Api/Program.cs`, `src/VaultApi.Api/appsettings.json`, `src/VaultApi.Api/appsettings.Development.json`
- Create: `tests/VaultApi.Tests/Fixtures/PostgresFixture.cs`
- Create: `tests/VaultApi.Tests/Fixtures/SmokeTests.cs`

**Interfaces:**
- Consumes: nothing yet (first Infrastructure code).
- Produces: `AppDbContext` (empty `DbContext` for now, entities added in later tasks), `AddInfrastructure(this IServiceCollection, IConfiguration)` extension method, `PostgresFixture` (xUnit `IAsyncLifetime` fixture wrapping a Testcontainers Postgres instance + a connection string) reused by every later integration test.

- [ ] **Step 1: Add NuGet packages**

```bash
dotnet add src/VaultApi.Infrastructure package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add src/VaultApi.Infrastructure package EFCore.NamingConventions
dotnet add src/VaultApi.Infrastructure package Microsoft.EntityFrameworkCore.Design
dotnet add src/VaultApi.Api package Microsoft.EntityFrameworkCore.Design
dotnet add tests/VaultApi.Tests package Testcontainers.PostgreSql
dotnet add tests/VaultApi.Tests package Microsoft.AspNetCore.Mvc.Testing
dotnet add tests/VaultApi.Tests package FluentAssertions
```

- [ ] **Step 2: Write the failing smoke test**

`tests/VaultApi.Tests/Fixtures/PostgresFixture.cs`:

```csharp
using Testcontainers.PostgreSql;

namespace VaultApi.Tests.Fixtures;

public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("vaultapi")
        .WithUsername("vaultapi")
        .WithPassword("vaultapi")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public Task InitializeAsync() => _container.StartAsync();

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();
}

[CollectionDefinition("Postgres")]
public sealed class PostgresCollection : ICollectionFixture<PostgresFixture>;
```

`tests/VaultApi.Tests/Fixtures/SmokeTests.cs`:

```csharp
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using VaultApi.Infrastructure.Persistence;

namespace VaultApi.Tests.Fixtures;

[Collection("Postgres")]
public class SmokeTests(PostgresFixture postgres)
{
    [Fact]
    public async Task Database_connects_and_can_be_created()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(postgres.ConnectionString)
            .UseSnakeCaseNamingConvention()
            .Options;

        await using var db = new AppDbContext(options);
        var canConnect = await db.Database.CanConnectAsync();

        canConnect.Should().BeTrue();
    }
}
```

- [ ] **Step 3: Run to verify it fails**

Run: `dotnet test tests/VaultApi.Tests --filter SmokeTests`
Expected: FAIL to compile — `AppDbContext` does not exist yet.

- [ ] **Step 4: Implement `AppDbContext` and DI wiring**

`src/VaultApi.Infrastructure/Persistence/AppDbContext.cs`:

```csharp
using Microsoft.EntityFrameworkCore;

namespace VaultApi.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
```

`src/VaultApi.Infrastructure/DependencyInjection.cs`:

```csharp
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
```

- [ ] **Step 5: Wire into the Api and add config**

In `src/VaultApi.Api/Program.cs`, add before `builder.Build()`:

```csharp
builder.Services.AddInfrastructure(builder.Configuration);
```

In `src/VaultApi.Api/appsettings.Development.json`, add:

```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Database=vaultapi;Username=vaultapi;Password=vaultapi"
  }
}
```

- [ ] **Step 6: Add docker-compose for local Postgres**

`docker-compose.yml`:

```yaml
services:
  postgres:
    image: postgres:16-alpine
    environment:
      POSTGRES_DB: vaultapi
      POSTGRES_USER: vaultapi
      POSTGRES_PASSWORD: vaultapi
    ports:
      - "5432:5432"
    volumes:
      - vaultapi_pgdata:/var/lib/postgresql/data

volumes:
  vaultapi_pgdata:
```

- [ ] **Step 7: Run the smoke test and verify it passes**

Run: `dotnet test tests/VaultApi.Tests --filter SmokeTests`
Expected: PASS (Testcontainers pulls/starts `postgres:16-alpine` automatically — requires Docker running locally).

- [ ] **Step 8: Verify full build**

Run: `dotnet build VaultApi.slnx`
Expected: `Build succeeded.`

- [ ] **Step 9: Commit**

```bash
git add -A
git commit -m "feat: add EF Core/Postgres infrastructure shell and Testcontainers fixture"
```

---

## Task 3: Domain Enums

**Files:**
- Create: `src/VaultApi.Domain/Enums/Nivel.cs`
- Create: `src/VaultApi.Domain/Enums/TipoUnidade.cs`
- Create: `src/VaultApi.Domain/Enums/TipoDesconto.cs`
- Create: `src/VaultApi.Domain/Enums/StatusLicenca.cs`
- Create: `src/VaultApi.Domain/Enums/EntidadeTipoCatalogo.cs`
- Create: `src/VaultApi.Domain/Enums/TipoValorCatalogo.cs`
- Test: `tests/VaultApi.Tests/Domain/EnumsTests.cs`

**Interfaces:**
- Produces: `Nivel { Admin, Revenda, Usuario }`, `TipoUnidade { Servidor, Estacao, PDA, PDV }`, `TipoDesconto { Fixo, Percentual, Valor }`, `StatusLicenca { Ativa, Revogada }`, `EntidadeTipoCatalogo { ProdutoPrecoUnidade, Modulo, ModuloVariante }`, `TipoValorCatalogo { Adesao, Mensalidade, AdicionalPorUnidade }` — every later task's entities reference these exact names.

- [ ] **Step 1: Write the failing test**

```csharp
using VaultApi.Domain.Enums;
using FluentAssertions;

namespace VaultApi.Tests.Domain;

public class EnumsTests
{
    [Fact]
    public void Nivel_has_exactly_three_values()
    {
        Enum.GetValues<Nivel>().Should().BeEquivalentTo([Nivel.Admin, Nivel.Revenda, Nivel.Usuario]);
    }

    [Fact]
    public void TipoUnidade_has_exactly_four_values()
    {
        Enum.GetValues<TipoUnidade>().Should()
            .BeEquivalentTo([TipoUnidade.Servidor, TipoUnidade.Estacao, TipoUnidade.PDA, TipoUnidade.PDV]);
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test tests/VaultApi.Tests --filter EnumsTests`
Expected: FAIL to compile — `VaultApi.Domain.Enums` namespace does not exist.

- [ ] **Step 3: Implement the enums**

```csharp
namespace VaultApi.Domain.Enums;

public enum Nivel { Admin, Revenda, Usuario }
```

```csharp
namespace VaultApi.Domain.Enums;

public enum TipoUnidade { Servidor, Estacao, PDA, PDV }
```

```csharp
namespace VaultApi.Domain.Enums;

public enum TipoDesconto { Fixo, Percentual, Valor }
```

```csharp
namespace VaultApi.Domain.Enums;

public enum StatusLicenca { Ativa, Revogada }
```

```csharp
namespace VaultApi.Domain.Enums;

public enum EntidadeTipoCatalogo { ProdutoPrecoUnidade, Modulo, ModuloVariante }
```

```csharp
namespace VaultApi.Domain.Enums;

public enum TipoValorCatalogo { Adesao, Mensalidade, AdicionalPorUnidade }
```

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test tests/VaultApi.Tests --filter EnumsTests`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add -A
git commit -m "feat: add domain enums"
```

---

## Task 4: Usuario Entity + ASP.NET Identity Integration

**Files:**
- Create: `src/VaultApi.Domain/Entities/Usuario.cs`
- Modify: `src/VaultApi.Infrastructure/Persistence/AppDbContext.cs`
- Create: `src/VaultApi.Infrastructure/Persistence/Configurations/UsuarioConfiguration.cs`
- Modify: `src/VaultApi.Infrastructure/DependencyInjection.cs`
- Modify: `src/VaultApi.Infrastructure/VaultApi.Infrastructure.csproj` (add Identity package)
- Create migration: `src/VaultApi.Infrastructure/Migrations/*_InitialIdentity.cs`
- Test: `tests/VaultApi.Tests/Identity/UsuarioPersistenceTests.cs`

**Interfaces:**
- Consumes: `AppDbContext` (Task 2), `Nivel` enum (Task 3).
- Produces: `Usuario : IdentityUser<Guid>` with `Nome` (string), `Nivel` (Nivel), `RevendaId` (Guid?) — Task 5 (login) and Task 8 (scope) depend on these exact property names.

- [ ] **Step 1: Add Identity package**

```bash
dotnet add src/VaultApi.Infrastructure package Microsoft.AspNetCore.Identity.EntityFrameworkCore
```

- [ ] **Step 2: Write the failing integration test**

```csharp
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using VaultApi.Domain.Entities;
using VaultApi.Domain.Enums;
using VaultApi.Infrastructure.Persistence;

namespace VaultApi.Tests.Identity;

[Collection("Postgres")]
public class UsuarioPersistenceTests(PostgresFixture postgres)
{
    [Fact]
    public async Task Usuario_roundtrips_with_nivel_and_revenda_id()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(postgres.ConnectionString)
            .UseSnakeCaseNamingConvention()
            .Options;

        await using (var db = new AppDbContext(options))
        {
            await db.Database.EnsureDeletedAsync();
            await db.Database.MigrateAsync();
        }

        var revendaId = Guid.NewGuid();

        await using (var db = new AppDbContext(options))
        {
            db.Users.Add(new Usuario
            {
                Id = Guid.NewGuid(),
                UserName = "revenda@teste.com",
                Email = "revenda@teste.com",
                Nome = "Usuario Revenda",
                Nivel = Nivel.Revenda,
                RevendaId = revendaId
            });
            await db.SaveChangesAsync();
        }

        await using (var db = new AppDbContext(options))
        {
            var usuario = await db.Users.SingleAsync();
            usuario.Nivel.Should().Be(Nivel.Revenda);
            usuario.RevendaId.Should().Be(revendaId);
        }
    }
}
```

- [ ] **Step 3: Run test to verify it fails**

Run: `dotnet test tests/VaultApi.Tests --filter UsuarioPersistenceTests`
Expected: FAIL to compile — `Usuario`, `db.Users`, and `MigrateAsync` target don't exist yet.

- [ ] **Step 4: Implement the `Usuario` entity**

```csharp
using Microsoft.AspNetCore.Identity;
using VaultApi.Domain.Enums;

namespace VaultApi.Domain.Entities;

public class Usuario : IdentityUser<Guid>
{
    public string Nome { get; set; } = string.Empty;
    public Nivel Nivel { get; set; }
    public Guid? RevendaId { get; set; }
}
```

- [ ] **Step 5: Turn `AppDbContext` into an `IdentityDbContext`**

Replace `src/VaultApi.Infrastructure/Persistence/AppDbContext.cs`:

```csharp
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
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
```

- [ ] **Step 6: Add the entity configuration**

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VaultApi.Domain.Entities;

namespace VaultApi.Infrastructure.Persistence.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.Property(u => u.Nome).IsRequired().HasMaxLength(200);
        builder.Property(u => u.Nivel).HasConversion<string>().HasMaxLength(20);
    }
}
```

- [ ] **Step 7: Register Identity in DI**

In `src/VaultApi.Infrastructure/DependencyInjection.cs`, add after `AddDbContext`:

```csharp
services.AddIdentityCore<Domain.Entities.Usuario>(options =>
    {
        options.Password.RequireNonAlphanumeric = false;
        options.User.RequireUniqueEmail = true;
    })
    .AddRoles<Microsoft.AspNetCore.Identity.IdentityRole<Guid>>()
    .AddEntityFrameworkStores<Persistence.AppDbContext>();
```

- [ ] **Step 8: Generate the migration**

```bash
dotnet ef migrations add InitialIdentity \
  --project src/VaultApi.Infrastructure \
  --startup-project src/VaultApi.Api \
  --output-dir Migrations
```

- [ ] **Step 9: Run test to verify it passes**

Run: `dotnet test tests/VaultApi.Tests --filter UsuarioPersistenceTests`
Expected: PASS.

- [ ] **Step 10: Commit**

```bash
git add -A
git commit -m "feat: add Usuario entity with ASP.NET Identity integration"
```

---

## Task 5: JWT Authentication

**Files:**
- Create: `src/VaultApi.Application/Abstractions/ITokenService.cs`
- Create: `src/VaultApi.Infrastructure/Auth/TokenService.cs`
- Modify: `src/VaultApi.Infrastructure/DependencyInjection.cs`
- Modify: `src/VaultApi.Api/Program.cs` (JWT bearer + SignInManager/UserManager registration)
- Modify: `src/VaultApi.Api/appsettings.json` (JWT settings)
- Create: `src/VaultApi.Api/Controllers/AuthController.cs`
- Create: `tests/VaultApi.Tests/Fixtures/ApiFactory.cs`
- Test: `tests/VaultApi.Tests/Auth/AuthEndpointTests.cs`

**Interfaces:**
- Consumes: `Usuario` (Task 4).
- Produces: `ITokenService.GerarToken(Usuario usuario) : string` (JWT with claims `sub`, `nivel`, `revenda_id`), `POST /auth/login` endpoint returning `{ "token": "..." }`. Task 6+ integration tests authenticate through this endpoint; Task 8's `ICurrentUser`/`IScopeFilter` reads the `nivel`/`revenda_id` claims this task emits.

- [ ] **Step 1: Add packages**

```bash
dotnet add src/VaultApi.Infrastructure package Microsoft.IdentityModel.Tokens
dotnet add src/VaultApi.Infrastructure package System.IdentityModel.Tokens.Jwt
dotnet add src/VaultApi.Api package Microsoft.AspNetCore.Authentication.JwtBearer
```

- [ ] **Step 2: Write the failing integration test**

`tests/VaultApi.Tests/Fixtures/ApiFactory.cs`:

```csharp
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace VaultApi.Tests.Fixtures;

public class ApiFactory(string connectionString) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = connectionString
            });
        });
    }
}
```

```csharp
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using VaultApi.Domain.Entities;
using VaultApi.Domain.Enums;
using VaultApi.Infrastructure.Persistence;
using VaultApi.Tests.Fixtures;

namespace VaultApi.Tests.Auth;

[Collection("Postgres")]
public class AuthEndpointTests(PostgresFixture postgres)
{
    [Fact]
    public async Task Login_returns_jwt_with_nivel_and_revenda_claims()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(postgres.ConnectionString).UseSnakeCaseNamingConvention().Options;
        await using (var db = new AppDbContext(options))
        {
            await db.Database.EnsureDeletedAsync();
            await db.Database.MigrateAsync();
        }

        await using var factory = new ApiFactory(postgres.ConnectionString);
        using var scope = factory.Services.CreateScope();
        var userManager = scope.ServiceProvider
            .GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<Usuario>>();
        var revendaId = Guid.NewGuid();
        await userManager.CreateAsync(new Usuario
        {
            UserName = "admin@teste.com",
            Email = "admin@teste.com",
            Nome = "Admin",
            Nivel = Nivel.Admin,
            RevendaId = null
        }, "Senha123!");

        using var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/auth/login", new { email = "admin@teste.com", senha = "Senha123!" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        body!.Token.Should().NotBeNullOrEmpty();
    }

    private record LoginResponse(string Token);
}
```

- [ ] **Step 3: Run test to verify it fails**

Run: `dotnet test tests/VaultApi.Tests --filter AuthEndpointTests`
Expected: FAIL to compile — `Program` not public/partial yet, `/auth/login` doesn't exist, `ITokenService` doesn't exist.

- [ ] **Step 4: Implement `ITokenService` and `TokenService`**

```csharp
using VaultApi.Domain.Entities;

namespace VaultApi.Application.Abstractions;

public interface ITokenService
{
    string GerarToken(Usuario usuario);
}
```

```csharp
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using VaultApi.Application.Abstractions;
using VaultApi.Domain.Entities;

namespace VaultApi.Infrastructure.Auth;

public class TokenService(IConfiguration configuration) : ITokenService
{
    public string GerarToken(Usuario usuario)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new("nivel", usuario.Nivel.ToString())
        };
        if (usuario.RevendaId is { } revendaId)
        {
            claims.Add(new Claim("revenda_id", revendaId.ToString()));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

- [ ] **Step 5: Register Identity sign-in support and `ITokenService`**

In `src/VaultApi.Infrastructure/DependencyInjection.cs`, change `AddIdentityCore` to also add sign-in support and register the token service:

```csharp
services.AddIdentityCore<Domain.Entities.Usuario>(options =>
    {
        options.Password.RequireNonAlphanumeric = false;
        options.User.RequireUniqueEmail = true;
    })
    .AddRoles<Microsoft.AspNetCore.Identity.IdentityRole<Guid>>()
    .AddSignInManager()
    .AddEntityFrameworkStores<Persistence.AppDbContext>();

services.AddScoped<Application.Abstractions.ITokenService, Auth.TokenService>();
```

- [ ] **Step 6: Configure JWT bearer auth and expose `Program` in the Api**

In `src/VaultApi.Api/appsettings.json`, add:

```json
{
  "Jwt": {
    "Key": "dev-only-signing-key-change-me-please-32chars-min",
    "Issuer": "VaultApi",
    "Audience": "VaultApi"
  }
}
```

In `src/VaultApi.Api/Program.cs`, add (after `AddInfrastructure`, before `builder.Build()`):

```csharp
builder.Services
    .AddAuthentication(Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });
builder.Services.AddAuthorization();
```

And after `var app = builder.Build();`, ensure `app.UseAuthentication();` is called before `app.UseAuthorization();`.

At the bottom of `Program.cs`, add (required so `WebApplicationFactory<Program>` can find the entry point):

```csharp
public partial class Program;
```

- [ ] **Step 7: Implement the login endpoint**

```csharp
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VaultApi.Application.Abstractions;
using VaultApi.Domain.Entities;

namespace VaultApi.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController(UserManager<Usuario> userManager, ITokenService tokenService) : ControllerBase
{
    public record LoginRequest(string Email, string Senha);
    public record LoginResponse(string Token);

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        var usuario = await userManager.FindByEmailAsync(request.Email);
        if (usuario is null || !await userManager.CheckPasswordAsync(usuario, request.Senha))
        {
            return Unauthorized();
        }

        return Ok(new LoginResponse(tokenService.GerarToken(usuario)));
    }
}
```

- [ ] **Step 8: Run test to verify it passes**

Run: `dotnet test tests/VaultApi.Tests --filter AuthEndpointTests`
Expected: PASS.

- [ ] **Step 9: Commit**

```bash
git add -A
git commit -m "feat: add JWT login endpoint with nivel/revenda_id claims"
```

---

## Task 6: Revenda CRUD (Admin-only write, all-authenticated read)

**Files:**
- Create: `src/VaultApi.Domain/Entities/Revenda.cs`
- Create: `src/VaultApi.Domain/Repositories/IRevendaRepository.cs`
- Create: `src/VaultApi.Infrastructure/Persistence/Configurations/RevendaConfiguration.cs`
- Create: `src/VaultApi.Infrastructure/Repositories/RevendaRepository.cs`
- Modify: `src/VaultApi.Infrastructure/DependencyInjection.cs`
- Create: `src/VaultApi.Application/Revendas/RevendaDtos.cs`, `src/VaultApi.Application/Revendas/RevendaService.cs`
- Create: `src/VaultApi.Api/Auth/PolicyNames.cs`
- Modify: `src/VaultApi.Api/Program.cs` (register `RequireAdmin` policy, register `RevendaService`)
- Create: `src/VaultApi.Api/Controllers/RevendasController.cs`
- Create migration: `src/VaultApi.Infrastructure/Migrations/*_AddRevenda.cs`
- Test: `tests/VaultApi.Tests/Revendas/RevendaEndpointTests.cs`

**Interfaces:**
- Consumes: `AppDbContext`, JWT auth (Task 5).
- Produces: `Revenda(Id, Nome, Cnpj, Ativo)`, `IRevendaRepository { Task<Revenda> AddAsync(Revenda); Task<Revenda?> GetAsync(Guid); Task<List<Revenda>> ListAsync(); Task SaveChangesAsync(); }`, `PolicyNames.RequireAdmin = "RequireAdmin"` constant reused by every later write endpoint, `POST/GET /revendas` endpoints.

- [ ] **Step 1: Write the failing integration test**

```csharp
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VaultApi.Domain.Entities;
using VaultApi.Domain.Enums;
using VaultApi.Infrastructure.Persistence;
using VaultApi.Tests.Fixtures;

namespace VaultApi.Tests.Revendas;

[Collection("Postgres")]
public class RevendaEndpointTests(PostgresFixture postgres)
{
    [Fact]
    public async Task Admin_can_create_and_list_revendas()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(postgres.ConnectionString).UseSnakeCaseNamingConvention().Options;
        await using (var db = new AppDbContext(options))
        {
            await db.Database.EnsureDeletedAsync();
            await db.Database.MigrateAsync();
        }

        await using var factory = new ApiFactory(postgres.ConnectionString);
        var token = await TestAuth.CreateUserAndLoginAsync(factory, Nivel.Admin, revendaId: null);

        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createResponse = await client.PostAsJsonAsync("/revendas", new { nome = "Revenda Teste", cnpj = "00000000000100" });
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var listResponse = await client.GetFromJsonAsync<List<RevendaDto>>("/revendas");
        listResponse.Should().ContainSingle(r => r.Nome == "Revenda Teste");
    }

    [Fact]
    public async Task NonAdmin_cannot_create_revenda()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(postgres.ConnectionString).UseSnakeCaseNamingConvention().Options;
        await using (var db = new AppDbContext(options))
        {
            await db.Database.EnsureDeletedAsync();
            await db.Database.MigrateAsync();
        }

        await using var factory = new ApiFactory(postgres.ConnectionString);
        var token = await TestAuth.CreateUserAndLoginAsync(factory, Nivel.Revenda, revendaId: Guid.NewGuid());

        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PostAsJsonAsync("/revendas", new { nome = "X", cnpj = "1" });
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private record RevendaDto(Guid Id, string Nome, string Cnpj, bool Ativo);
}
```

`tests/VaultApi.Tests/Fixtures/TestAuth.cs` (shared helper, used by this and every later scope test):

```csharp
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using VaultApi.Application.Abstractions;
using VaultApi.Domain.Entities;
using VaultApi.Domain.Enums;

namespace VaultApi.Tests.Fixtures;

public static class TestAuth
{
    public static async Task<string> CreateUserAndLoginAsync(ApiFactory factory, Nivel nivel, Guid? revendaId)
    {
        using var scope = factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Usuario>>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        var email = $"{Guid.NewGuid()}@teste.com";
        var usuario = new Usuario { UserName = email, Email = email, Nome = "Teste", Nivel = nivel, RevendaId = revendaId };
        var result = await userManager.CreateAsync(usuario, "Senha123!");
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join(";", result.Errors.Select(e => e.Description)));
        }

        return tokenService.GerarToken(usuario);
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test tests/VaultApi.Tests --filter RevendaEndpointTests`
Expected: FAIL to compile — `/revendas` controller and `Revenda` entity don't exist.

- [ ] **Step 3: Implement the `Revenda` entity and repository**

```csharp
namespace VaultApi.Domain.Entities;

public class Revenda
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Cnpj { get; set; } = string.Empty;
    public bool Ativo { get; set; } = true;
}
```

```csharp
using VaultApi.Domain.Entities;

namespace VaultApi.Domain.Repositories;

public interface IRevendaRepository
{
    Task AddAsync(Revenda revenda);
    Task<Revenda?> GetAsync(Guid id);
    Task<List<Revenda>> ListAsync();
    Task SaveChangesAsync();
}
```

```csharp
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
```

```csharp
using Microsoft.EntityFrameworkCore;
using VaultApi.Domain.Entities;
using VaultApi.Domain.Repositories;
using VaultApi.Infrastructure.Persistence;

namespace VaultApi.Infrastructure.Repositories;

public class RevendaRepository(AppDbContext db) : IRevendaRepository
{
    public async Task AddAsync(Revenda revenda) => await db.Set<Revenda>().AddAsync(revenda);
    public Task<Revenda?> GetAsync(Guid id) => db.Set<Revenda>().SingleOrDefaultAsync(r => r.Id == id);
    public Task<List<Revenda>> ListAsync() => db.Set<Revenda>().OrderBy(r => r.Nome).ToListAsync();
    public Task SaveChangesAsync() => db.SaveChangesAsync();
}
```

Register in `src/VaultApi.Infrastructure/DependencyInjection.cs`:

```csharp
services.AddScoped<Domain.Repositories.IRevendaRepository, Repositories.RevendaRepository>();
```

- [ ] **Step 4: Implement the Application service and DTOs**

```csharp
namespace VaultApi.Application.Revendas;

public record CriarRevendaRequest(string Nome, string Cnpj);
public record RevendaResponse(Guid Id, string Nome, string Cnpj, bool Ativo);
```

```csharp
using VaultApi.Domain.Entities;
using VaultApi.Domain.Repositories;

namespace VaultApi.Application.Revendas;

public class RevendaService(IRevendaRepository repository)
{
    public async Task<RevendaResponse> CriarAsync(CriarRevendaRequest request)
    {
        var revenda = new Revenda { Id = Guid.NewGuid(), Nome = request.Nome, Cnpj = request.Cnpj, Ativo = true };
        await repository.AddAsync(revenda);
        await repository.SaveChangesAsync();
        return new RevendaResponse(revenda.Id, revenda.Nome, revenda.Cnpj, revenda.Ativo);
    }

    public async Task<List<RevendaResponse>> ListarAsync()
    {
        var revendas = await repository.ListAsync();
        return revendas.Select(r => new RevendaResponse(r.Id, r.Nome, r.Cnpj, r.Ativo)).ToList();
    }
}
```

Register in `src/VaultApi.Infrastructure/DependencyInjection.cs` (Application services can be registered from Infrastructure's composition root, called from `Program.cs`):

```csharp
services.AddScoped<Application.Revendas.RevendaService>();
```

- [ ] **Step 5: Add the `RequireAdmin` policy and controller**

```csharp
namespace VaultApi.Api.Auth;

public static class PolicyNames
{
    public const string RequireAdmin = "RequireAdmin";
    public const string RequireRevendaOrAdmin = "RequireRevendaOrAdmin";
}
```

In `src/VaultApi.Api/Program.cs`, replace `builder.Services.AddAuthorization();` with:

```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(VaultApi.Api.Auth.PolicyNames.RequireAdmin, policy =>
        policy.RequireClaim("nivel", nameof(VaultApi.Domain.Enums.Nivel.Admin)));
    options.AddPolicy(VaultApi.Api.Auth.PolicyNames.RequireRevendaOrAdmin, policy =>
        policy.RequireClaim("nivel",
            nameof(VaultApi.Domain.Enums.Nivel.Admin),
            nameof(VaultApi.Domain.Enums.Nivel.Revenda),
            nameof(VaultApi.Domain.Enums.Nivel.Usuario)));
});
```

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VaultApi.Api.Auth;
using VaultApi.Application.Revendas;

namespace VaultApi.Api.Controllers;

[ApiController]
[Route("revendas")]
[Authorize]
public class RevendasController(RevendaService service) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = PolicyNames.RequireAdmin)]
    public async Task<ActionResult<RevendaResponse>> Criar(CriarRevendaRequest request)
    {
        var revenda = await service.CriarAsync(request);
        return CreatedAtAction(nameof(Listar), new { }, revenda);
    }

    [HttpGet]
    [Authorize(Policy = PolicyNames.RequireRevendaOrAdmin)]
    public async Task<ActionResult<List<RevendaResponse>>> Listar() => Ok(await service.ListarAsync());
}
```

- [ ] **Step 6: Generate the migration**

```bash
dotnet ef migrations add AddRevenda \
  --project src/VaultApi.Infrastructure \
  --startup-project src/VaultApi.Api \
  --output-dir Migrations
```

- [ ] **Step 7: Run tests to verify they pass**

Run: `dotnet test tests/VaultApi.Tests --filter RevendaEndpointTests`
Expected: PASS (both tests).

- [ ] **Step 8: Commit**

```bash
git add -A
git commit -m "feat: add Revenda CRUD with RequireAdmin/RequireRevendaOrAdmin policies"
```

---

## Task 7: Cliente CRUD (Admin-only write)

**Files:**
- Create: `src/VaultApi.Domain/Entities/Cliente.cs`
- Create: `src/VaultApi.Domain/Repositories/IClienteRepository.cs`
- Create: `src/VaultApi.Infrastructure/Persistence/Configurations/ClienteConfiguration.cs`
- Create: `src/VaultApi.Infrastructure/Repositories/ClienteRepository.cs`
- Modify: `src/VaultApi.Infrastructure/DependencyInjection.cs`
- Create: `src/VaultApi.Application/Clientes/ClienteDtos.cs`, `src/VaultApi.Application/Clientes/ClienteService.cs`
- Create: `src/VaultApi.Api/Controllers/ClientesController.cs`
- Create migration: `src/VaultApi.Infrastructure/Migrations/*_AddCliente.cs`
- Test: `tests/VaultApi.Tests/Clientes/ClienteEndpointTests.cs`

**Interfaces:**
- Consumes: `Revenda` (Task 6), auth policies (Task 6).
- Produces: `Cliente(Id, Nome, Cnpj, RevendaId?)`, `IClienteRepository { AddAsync, GetAsync, ListAllAsync, SaveChangesAsync }` — `ListAllAsync` here is unscoped; Task 8 adds the scoped variant used by the controller.

- [ ] **Step 1: Write the failing integration test**

```csharp
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using VaultApi.Domain.Enums;
using VaultApi.Infrastructure.Persistence;
using VaultApi.Tests.Fixtures;

namespace VaultApi.Tests.Clientes;

[Collection("Postgres")]
public class ClienteEndpointTests(PostgresFixture postgres)
{
    [Fact]
    public async Task Admin_can_create_cliente_with_and_without_revenda()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(postgres.ConnectionString).UseSnakeCaseNamingConvention().Options;
        await using (var db = new AppDbContext(options))
        {
            await db.Database.EnsureDeletedAsync();
            await db.Database.MigrateAsync();
        }

        await using var factory = new ApiFactory(postgres.ConnectionString);
        var token = await TestAuth.CreateUserAndLoginAsync(factory, Nivel.Admin, revendaId: null);
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var matrizResponse = await client.PostAsJsonAsync("/clientes", new { nome = "Cliente Matriz", cnpj = "1", revendaId = (Guid?)null });
        matrizResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var revendaId = Guid.NewGuid();
        await using (var db = new AppDbContext(options))
        {
            db.Set<VaultApi.Domain.Entities.Revenda>().Add(new VaultApi.Domain.Entities.Revenda { Id = revendaId, Nome = "R1", Cnpj = "2", Ativo = true });
            await db.SaveChangesAsync();
        }

        var comRevendaResponse = await client.PostAsJsonAsync("/clientes", new { nome = "Cliente Revenda", cnpj = "3", revendaId });
        comRevendaResponse.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test tests/VaultApi.Tests --filter ClienteEndpointTests`
Expected: FAIL to compile — `/clientes` and `Cliente` don't exist yet.

- [ ] **Step 3: Implement entity, repository, configuration**

```csharp
namespace VaultApi.Domain.Entities;

public class Cliente
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Cnpj { get; set; } = string.Empty;
    public Guid? RevendaId { get; set; }
}
```

```csharp
using VaultApi.Domain.Entities;

namespace VaultApi.Domain.Repositories;

public interface IClienteRepository
{
    Task AddAsync(Cliente cliente);
    Task<Cliente?> GetAsync(Guid id);
    Task<List<Cliente>> ListAllAsync();
    Task SaveChangesAsync();
}
```

```csharp
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
```

```csharp
using Microsoft.EntityFrameworkCore;
using VaultApi.Domain.Entities;
using VaultApi.Domain.Repositories;
using VaultApi.Infrastructure.Persistence;

namespace VaultApi.Infrastructure.Repositories;

public class ClienteRepository(AppDbContext db) : IClienteRepository
{
    public async Task AddAsync(Cliente cliente) => await db.Set<Cliente>().AddAsync(cliente);
    public Task<Cliente?> GetAsync(Guid id) => db.Set<Cliente>().SingleOrDefaultAsync(c => c.Id == id);
    public Task<List<Cliente>> ListAllAsync() => db.Set<Cliente>().OrderBy(c => c.Nome).ToListAsync();
    public Task SaveChangesAsync() => db.SaveChangesAsync();
}
```

Register: `services.AddScoped<Domain.Repositories.IClienteRepository, Repositories.ClienteRepository>();`

- [ ] **Step 4: Implement service, DTOs, controller**

```csharp
namespace VaultApi.Application.Clientes;

public record CriarClienteRequest(string Nome, string Cnpj, Guid? RevendaId);
public record ClienteResponse(Guid Id, string Nome, string Cnpj, Guid? RevendaId);
```

```csharp
using VaultApi.Domain.Entities;
using VaultApi.Domain.Repositories;

namespace VaultApi.Application.Clientes;

public class ClienteService(IClienteRepository repository)
{
    public async Task<ClienteResponse> CriarAsync(CriarClienteRequest request)
    {
        var cliente = new Cliente { Id = Guid.NewGuid(), Nome = request.Nome, Cnpj = request.Cnpj, RevendaId = request.RevendaId };
        await repository.AddAsync(cliente);
        await repository.SaveChangesAsync();
        return new ClienteResponse(cliente.Id, cliente.Nome, cliente.Cnpj, cliente.RevendaId);
    }
}
```

Register: `services.AddScoped<Application.Clientes.ClienteService>();`

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VaultApi.Api.Auth;
using VaultApi.Application.Clientes;

namespace VaultApi.Api.Controllers;

[ApiController]
[Route("clientes")]
[Authorize]
public class ClientesController(ClienteService service) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = PolicyNames.RequireAdmin)]
    public async Task<ActionResult<ClienteResponse>> Criar(CriarClienteRequest request)
    {
        var cliente = await service.CriarAsync(request);
        return Created($"/clientes/{cliente.Id}", cliente);
    }
}
```

- [ ] **Step 5: Generate the migration**

```bash
dotnet ef migrations add AddCliente \
  --project src/VaultApi.Infrastructure \
  --startup-project src/VaultApi.Api \
  --output-dir Migrations
```

- [ ] **Step 6: Run test to verify it passes**

Run: `dotnet test tests/VaultApi.Tests --filter ClienteEndpointTests`
Expected: PASS.

- [ ] **Step 7: Commit**

```bash
git add -A
git commit -m "feat: add Cliente CRUD (Admin-only write)"
```

---

## Task 8: Scope Filter (Revenda/Usuario data isolation)

**Files:**
- Create: `src/VaultApi.Application/Abstractions/ScopeResult.cs`
- Create: `src/VaultApi.Application/Abstractions/ICurrentUser.cs`
- Create: `src/VaultApi.Application/Abstractions/IScopeFilter.cs`
- Create: `src/VaultApi.Application/Scope/ScopeFilter.cs`
- Create: `src/VaultApi.Api/Auth/CurrentUser.cs`
- Modify: `src/VaultApi.Infrastructure/DependencyInjection.cs` (register `IScopeFilter`)
- Modify: `src/VaultApi.Api/Program.cs` (register `ICurrentUser`, `IHttpContextAccessor`)
- Modify: `src/VaultApi.Domain/Repositories/IClienteRepository.cs` (add scoped list method)
- Modify: `src/VaultApi.Infrastructure/Repositories/ClienteRepository.cs`
- Modify: `src/VaultApi.Application/Clientes/ClienteService.cs` (add `ListarAsync` using scope)
- Modify: `src/VaultApi.Api/Controllers/ClientesController.cs` (add `GET /clientes`)
- Test: `tests/VaultApi.Tests/Scope/ScopeFilterTests.cs` (pure unit test)
- Test: `tests/VaultApi.Tests/Clientes/ClienteScopeEndpointTests.cs` (integration test)

**Interfaces:**
- Consumes: `Nivel`, `Usuario.RevendaId` claim (Task 4/5).
- Produces: `ScopeResult` (abstract record: `SemRestricao` and `RestritoARevenda(Guid? RevendaId)`), `ICurrentUser { Nivel Nivel; Guid? RevendaId; }`, `IScopeFilter { ScopeResult Resolve(); }` — Task 13's `ContratoService` and this task's `ClienteService.ListarAsync` both consume `IScopeFilter.Resolve()` the same way.

- [ ] **Step 1: Write the failing pure unit test**

```csharp
using FluentAssertions;
using VaultApi.Application.Abstractions;
using VaultApi.Application.Scope;
using VaultApi.Domain.Enums;

namespace VaultApi.Tests.Scope;

public class ScopeFilterTests
{
    private class FakeCurrentUser(Nivel nivel, Guid? revendaId) : ICurrentUser
    {
        public Nivel Nivel => nivel;
        public Guid? RevendaId => revendaId;
    }

    [Fact]
    public void Admin_has_no_restriction()
    {
        var filter = new ScopeFilter(new FakeCurrentUser(Nivel.Admin, null));
        filter.Resolve().Should().BeOfType<ScopeResult.SemRestricao>();
    }

    [Fact]
    public void Revenda_user_is_restricted_to_own_revenda_id()
    {
        var revendaId = Guid.NewGuid();
        var filter = new ScopeFilter(new FakeCurrentUser(Nivel.Revenda, revendaId));
        var result = filter.Resolve().Should().BeOfType<ScopeResult.RestritoARevenda>().Subject;
        result.RevendaId.Should().Be(revendaId);
    }

    [Fact]
    public void Usuario_from_matriz_is_restricted_to_null_revenda_id()
    {
        var filter = new ScopeFilter(new FakeCurrentUser(Nivel.Usuario, null));
        var result = filter.Resolve().Should().BeOfType<ScopeResult.RestritoARevenda>().Subject;
        result.RevendaId.Should().BeNull();
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test tests/VaultApi.Tests --filter ScopeFilterTests`
Expected: FAIL to compile — `ScopeResult`, `ICurrentUser`, `ScopeFilter` don't exist.

- [ ] **Step 3: Implement `ScopeResult`, `ICurrentUser`, `IScopeFilter`, `ScopeFilter`**

```csharp
namespace VaultApi.Application.Abstractions;

public abstract record ScopeResult
{
    public sealed record SemRestricao : ScopeResult;
    public sealed record RestritoARevenda(Guid? RevendaId) : ScopeResult;
}
```

```csharp
using VaultApi.Domain.Enums;

namespace VaultApi.Application.Abstractions;

public interface ICurrentUser
{
    Nivel Nivel { get; }
    Guid? RevendaId { get; }
}
```

```csharp
namespace VaultApi.Application.Abstractions;

public interface IScopeFilter
{
    ScopeResult Resolve();
}
```

```csharp
using VaultApi.Application.Abstractions;
using VaultApi.Domain.Enums;

namespace VaultApi.Application.Scope;

public class ScopeFilter(ICurrentUser currentUser) : IScopeFilter
{
    public ScopeResult Resolve() => currentUser.Nivel switch
    {
        Nivel.Admin => new ScopeResult.SemRestricao(),
        _ => new ScopeResult.RestritoARevenda(currentUser.RevendaId)
    };
}
```

- [ ] **Step 4: Run unit test to verify it passes**

Run: `dotnet test tests/VaultApi.Tests --filter ScopeFilterTests`
Expected: PASS.

- [ ] **Step 5: Write the failing integration test for scoped Cliente listing**

```csharp
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using VaultApi.Domain.Entities;
using VaultApi.Domain.Enums;
using VaultApi.Infrastructure.Persistence;
using VaultApi.Tests.Fixtures;

namespace VaultApi.Tests.Clientes;

[Collection("Postgres")]
public class ClienteScopeEndpointTests(PostgresFixture postgres)
{
    [Fact]
    public async Task Revenda_user_only_sees_clientes_of_own_revenda()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(postgres.ConnectionString).UseSnakeCaseNamingConvention().Options;
        await using (var db = new AppDbContext(options))
        {
            await db.Database.EnsureDeletedAsync();
            await db.Database.MigrateAsync();
        }

        var revendaA = Guid.NewGuid();
        var revendaB = Guid.NewGuid();
        await using (var db = new AppDbContext(options))
        {
            db.Set<Revenda>().AddRange(
                new Revenda { Id = revendaA, Nome = "A", Cnpj = "1", Ativo = true },
                new Revenda { Id = revendaB, Nome = "B", Cnpj = "2", Ativo = true });
            db.Set<Cliente>().AddRange(
                new Cliente { Id = Guid.NewGuid(), Nome = "Cliente A", Cnpj = "10", RevendaId = revendaA },
                new Cliente { Id = Guid.NewGuid(), Nome = "Cliente B", Cnpj = "20", RevendaId = revendaB },
                new Cliente { Id = Guid.NewGuid(), Nome = "Cliente Matriz", Cnpj = "30", RevendaId = null });
            await db.SaveChangesAsync();
        }

        await using var factory = new ApiFactory(postgres.ConnectionString);
        var token = await TestAuth.CreateUserAndLoginAsync(factory, Nivel.Revenda, revendaA);
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var clientes = await client.GetFromJsonAsync<List<ClienteDto>>("/clientes");

        clientes.Should().ContainSingle().Which.Nome.Should().Be("Cliente A");
    }

    private record ClienteDto(Guid Id, string Nome, string Cnpj, Guid? RevendaId);
}
```

- [ ] **Step 6: Run test to verify it fails**

Run: `dotnet test tests/VaultApi.Tests --filter ClienteScopeEndpointTests`
Expected: FAIL — `GET /clientes` doesn't exist yet.

- [ ] **Step 7: Implement `ICurrentUser` (Api-side, reads `HttpContext`)**

```csharp
using Microsoft.AspNetCore.Http;
using VaultApi.Application.Abstractions;
using VaultApi.Domain.Enums;

namespace VaultApi.Api.Auth;

public class CurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    public Nivel Nivel => Enum.Parse<Nivel>(
        accessor.HttpContext?.User.FindFirst("nivel")?.Value
        ?? throw new InvalidOperationException("Requisicao sem claim 'nivel'."));

    public Guid? RevendaId
    {
        get
        {
            var value = accessor.HttpContext?.User.FindFirst("revenda_id")?.Value;
            return value is null ? null : Guid.Parse(value);
        }
    }
}
```

- [ ] **Step 8: Register `ICurrentUser` and `IScopeFilter` in DI**

In `src/VaultApi.Api/Program.cs`, add before `builder.Build()`:

```csharp
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<VaultApi.Application.Abstractions.ICurrentUser, VaultApi.Api.Auth.CurrentUser>();
```

In `src/VaultApi.Infrastructure/DependencyInjection.cs`, add:

```csharp
services.AddScoped<Application.Abstractions.IScopeFilter, Application.Scope.ScopeFilter>();
```

- [ ] **Step 9: Add scoped listing to `IClienteRepository`/`ClienteRepository`**

Add to `IClienteRepository`. Domain cannot reference Application (would be circular), so the scope is passed as a plain `bool`/`Guid?` pair rather than the `ScopeResult` type itself:

```csharp
Task<List<Cliente>> ListAsync(bool semRestricao, Guid? revendaId);
```

Implement in `ClienteRepository`:

```csharp
public Task<List<Cliente>> ListAsync(bool semRestricao, Guid? revendaId) => db.Set<Cliente>()
    .Where(c => semRestricao || c.RevendaId == revendaId)
    .OrderBy(c => c.Nome)
    .ToListAsync();
```

- [ ] **Step 10: Wire the scope through `ClienteService` and add `GET /clientes`**

Add to `ClienteService`:

```csharp
public async Task<List<ClienteResponse>> ListarAsync(ScopeResult scope)
{
    var (semRestricao, revendaId) = scope switch
    {
        ScopeResult.SemRestricao => (true, (Guid?)null),
        ScopeResult.RestritoARevenda r => (false, r.RevendaId),
        _ => throw new InvalidOperationException()
    };

    var clientes = await repository.ListAsync(semRestricao, revendaId);
    return clientes.Select(c => new ClienteResponse(c.Id, c.Nome, c.Cnpj, c.RevendaId)).ToList();
}
```

(Add `using VaultApi.Application.Abstractions;` to `ClienteService.cs`.)

Add to `ClientesController`:

```csharp
private readonly IScopeFilter _scopeFilter;

// add IScopeFilter scopeFilter to the primary constructor:
// public class ClientesController(ClienteService service, IScopeFilter scopeFilter) : ControllerBase

[HttpGet]
[Authorize(Policy = PolicyNames.RequireRevendaOrAdmin)]
public async Task<ActionResult<List<ClienteResponse>>> Listar() =>
    Ok(await service.ListarAsync(scopeFilter.Resolve()));
```

(Replace the controller's primary constructor with `ClientesController(ClienteService service, IScopeFilter scopeFilter)` and add `using VaultApi.Application.Abstractions;`.)

- [ ] **Step 11: Run tests to verify they pass**

Run: `dotnet test tests/VaultApi.Tests --filter "ScopeFilterTests|ClienteScopeEndpointTests"`
Expected: PASS.

- [ ] **Step 12: Run full suite and commit**

Run: `dotnet test`
Expected: all tests PASS.

```bash
git add -A
git commit -m "feat: add IScopeFilter and enforce Revenda/Usuario data isolation on Cliente listing"
```

---

## Task 9: Catalog Entities (Produto, ProdutoPrecoUnidade, Modulo, ModuloVariante)

**Files:**
- Create: `src/VaultApi.Domain/Entities/Produto.cs`, `ProdutoPrecoUnidade.cs`, `Modulo.cs`, `ModuloVariante.cs`
- Create: `src/VaultApi.Domain/Repositories/IProdutoRepository.cs`
- Create: `src/VaultApi.Infrastructure/Persistence/Configurations/ProdutoConfiguration.cs`, `ProdutoPrecoUnidadeConfiguration.cs`, `ModuloConfiguration.cs`, `ModuloVarianteConfiguration.cs`
- Create: `src/VaultApi.Infrastructure/Repositories/ProdutoRepository.cs`
- Modify: `src/VaultApi.Infrastructure/DependencyInjection.cs`
- Create: `src/VaultApi.Application/Catalogo/CatalogoDtos.cs`, `src/VaultApi.Application/Catalogo/CatalogoService.cs`
- Create: `src/VaultApi.Api/Controllers/CatalogoController.cs`
- Create migration: `src/VaultApi.Infrastructure/Migrations/*_AddCatalogo.cs`
- Test: `tests/VaultApi.Tests/Catalogo/CatalogoEndpointTests.cs`

**Interfaces:**
- Consumes: auth policies (Task 6).
- Produces: `Produto(Id, Nome, Descricao, Ativo)`, `ProdutoPrecoUnidade(Id, ProdutoId, TipoUnidade, ValorAdesao, ValorMensalidade)`, `Modulo(Id, ProdutoId, Nome, ValorAdesaoBase?, ValorMensalidadeBase?, Ativo)`, `ModuloVariante(Id, ModuloId, Nome, TipoUnidadeAplicavel, ValorAdicionalPorUnidade)`, `IProdutoRepository { AddAsync, GetAsync (with Precos/Modulos/Variantes included), ListAsync, SaveChangesAsync }`. Task 11 (Contrato) and Task 12 (PricingResolver) consume these exact entity shapes.

- [ ] **Step 1: Write the failing integration test**

```csharp
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using VaultApi.Domain.Enums;
using VaultApi.Infrastructure.Persistence;
using VaultApi.Tests.Fixtures;

namespace VaultApi.Tests.Catalogo;

[Collection("Postgres")]
public class CatalogoEndpointTests(PostgresFixture postgres)
{
    [Fact]
    public async Task Admin_can_create_produto_with_precos_por_unidade_and_modulo_com_variante()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(postgres.ConnectionString).UseSnakeCaseNamingConvention().Options;
        await using (var db = new AppDbContext(options))
        {
            await db.Database.EnsureDeletedAsync();
            await db.Database.MigrateAsync();
        }

        await using var factory = new ApiFactory(postgres.ConnectionString);
        var token = await TestAuth.CreateUserAndLoginAsync(factory, Nivel.Admin, revendaId: null);
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var criarProdutoResponse = await client.PostAsJsonAsync("/produtos", new
        {
            nome = "MidPRO",
            descricao = "Produto de teste",
            precosPorUnidade = new[]
            {
                new { tipoUnidade = "Servidor", valorAdesao = 500m, valorMensalidade = 100m },
                new { tipoUnidade = "PDV", valorAdesao = 50m, valorMensalidade = 20m }
            }
        });
        criarProdutoResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var produto = await criarProdutoResponse.Content.ReadFromJsonAsync<ProdutoResponse>();

        var criarModuloResponse = await client.PostAsJsonAsync($"/produtos/{produto!.Id}/modulos", new
        {
            nome = "TEF",
            valorAdesaoBase = (decimal?)null,
            valorMensalidadeBase = (decimal?)null,
            variantes = new[]
            {
                new { nome = "TEF-Sitef", tipoUnidadeAplicavel = "PDV", valorAdicionalPorUnidade = 15m }
            }
        });
        criarModuloResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var produtoCompleto = await client.GetFromJsonAsync<ProdutoResponse>($"/produtos/{produto.Id}");
        produtoCompleto!.PrecosPorUnidade.Should().HaveCount(2);
        produtoCompleto.Modulos.Should().ContainSingle(m => m.Nome == "TEF" && m.Variantes.Count == 1);
    }

    private record ProdutoResponse(Guid Id, string Nome, string Descricao, bool Ativo,
        List<PrecoUnidadeResponse> PrecosPorUnidade, List<ModuloResponse> Modulos);
    private record PrecoUnidadeResponse(string TipoUnidade, decimal ValorAdesao, decimal ValorMensalidade);
    private record ModuloResponse(Guid Id, string Nome, decimal? ValorAdesaoBase, decimal? ValorMensalidadeBase,
        bool Ativo, List<VarianteResponse> Variantes);
    private record VarianteResponse(Guid Id, string Nome, string TipoUnidadeAplicavel, decimal ValorAdicionalPorUnidade);
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test tests/VaultApi.Tests --filter CatalogoEndpointTests`
Expected: FAIL to compile — catalog entities/endpoints don't exist.

- [ ] **Step 3: Implement the entities**

```csharp
namespace VaultApi.Domain.Entities;

public class Produto
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public bool Ativo { get; set; } = true;
    public List<ProdutoPrecoUnidade> PrecosPorUnidade { get; set; } = [];
    public List<Modulo> Modulos { get; set; } = [];
}
```

```csharp
using VaultApi.Domain.Enums;

namespace VaultApi.Domain.Entities;

public class ProdutoPrecoUnidade
{
    public Guid Id { get; set; }
    public Guid ProdutoId { get; set; }
    public TipoUnidade TipoUnidade { get; set; }
    public decimal ValorAdesao { get; set; }
    public decimal ValorMensalidade { get; set; }
}
```

```csharp
namespace VaultApi.Domain.Entities;

public class Modulo
{
    public Guid Id { get; set; }
    public Guid ProdutoId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public decimal? ValorAdesaoBase { get; set; }
    public decimal? ValorMensalidadeBase { get; set; }
    public bool Ativo { get; set; } = true;
    public List<ModuloVariante> Variantes { get; set; } = [];
}
```

```csharp
using VaultApi.Domain.Enums;

namespace VaultApi.Domain.Entities;

public class ModuloVariante
{
    public Guid Id { get; set; }
    public Guid ModuloId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public TipoUnidade TipoUnidadeAplicavel { get; set; }
    public decimal ValorAdicionalPorUnidade { get; set; }
}
```

- [ ] **Step 4: Implement EF configurations**

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VaultApi.Domain.Entities;

namespace VaultApi.Infrastructure.Persistence.Configurations;

public class ProdutoConfiguration : IEntityTypeConfiguration<Produto>
{
    public void Configure(EntityTypeBuilder<Produto> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Nome).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Descricao).HasMaxLength(1000);
        builder.HasMany(p => p.PrecosPorUnidade).WithOne().HasForeignKey(pu => pu.ProdutoId);
        builder.HasMany(p => p.Modulos).WithOne().HasForeignKey(m => m.ProdutoId);
    }
}
```

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VaultApi.Domain.Entities;

namespace VaultApi.Infrastructure.Persistence.Configurations;

public class ProdutoPrecoUnidadeConfiguration : IEntityTypeConfiguration<ProdutoPrecoUnidade>
{
    public void Configure(EntityTypeBuilder<ProdutoPrecoUnidade> builder)
    {
        builder.HasKey(pu => pu.Id);
        builder.Property(pu => pu.TipoUnidade).HasConversion<string>().HasMaxLength(20);
        builder.HasIndex(pu => new { pu.ProdutoId, pu.TipoUnidade }).IsUnique();
    }
}
```

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VaultApi.Domain.Entities;

namespace VaultApi.Infrastructure.Persistence.Configurations;

public class ModuloConfiguration : IEntityTypeConfiguration<Modulo>
{
    public void Configure(EntityTypeBuilder<Modulo> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Nome).IsRequired().HasMaxLength(200);
        builder.HasMany(m => m.Variantes).WithOne().HasForeignKey(v => v.ModuloId);
    }
}
```

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VaultApi.Domain.Entities;

namespace VaultApi.Infrastructure.Persistence.Configurations;

public class ModuloVarianteConfiguration : IEntityTypeConfiguration<ModuloVariante>
{
    public void Configure(EntityTypeBuilder<ModuloVariante> builder)
    {
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Nome).IsRequired().HasMaxLength(200);
        builder.Property(v => v.TipoUnidadeAplicavel).HasConversion<string>().HasMaxLength(20);
    }
}
```

- [ ] **Step 5: Implement repository**

```csharp
using VaultApi.Domain.Entities;

namespace VaultApi.Domain.Repositories;

public interface IProdutoRepository
{
    Task AddAsync(Produto produto);
    Task<Produto?> GetAsync(Guid id);
    Task<List<Produto>> ListAsync();
    Task SaveChangesAsync();
}
```

```csharp
using Microsoft.EntityFrameworkCore;
using VaultApi.Domain.Entities;
using VaultApi.Domain.Repositories;
using VaultApi.Infrastructure.Persistence;

namespace VaultApi.Infrastructure.Repositories;

public class ProdutoRepository(AppDbContext db) : IProdutoRepository
{
    public async Task AddAsync(Produto produto) => await db.Set<Produto>().AddAsync(produto);

    public Task<Produto?> GetAsync(Guid id) => db.Set<Produto>()
        .Include(p => p.PrecosPorUnidade)
        .Include(p => p.Modulos).ThenInclude(m => m.Variantes)
        .SingleOrDefaultAsync(p => p.Id == id);

    public Task<List<Produto>> ListAsync() => db.Set<Produto>()
        .Include(p => p.PrecosPorUnidade)
        .Include(p => p.Modulos).ThenInclude(m => m.Variantes)
        .OrderBy(p => p.Nome)
        .ToListAsync();

    public Task SaveChangesAsync() => db.SaveChangesAsync();
}
```

Register: `services.AddScoped<Domain.Repositories.IProdutoRepository, Repositories.ProdutoRepository>();`

- [ ] **Step 6: Implement DTOs and service**

```csharp
using VaultApi.Domain.Enums;

namespace VaultApi.Application.Catalogo;

public record PrecoUnidadeRequest(TipoUnidade TipoUnidade, decimal ValorAdesao, decimal ValorMensalidade);
public record CriarProdutoRequest(string Nome, string Descricao, List<PrecoUnidadeRequest> PrecosPorUnidade);

public record VarianteRequest(string Nome, TipoUnidade TipoUnidadeAplicavel, decimal ValorAdicionalPorUnidade);
public record CriarModuloRequest(string Nome, decimal? ValorAdesaoBase, decimal? ValorMensalidadeBase, List<VarianteRequest> Variantes);

public record PrecoUnidadeResponse(TipoUnidade TipoUnidade, decimal ValorAdesao, decimal ValorMensalidade);
public record VarianteResponse(Guid Id, string Nome, TipoUnidade TipoUnidadeAplicavel, decimal ValorAdicionalPorUnidade);
public record ModuloResponse(Guid Id, string Nome, decimal? ValorAdesaoBase, decimal? ValorMensalidadeBase, bool Ativo, List<VarianteResponse> Variantes);
public record ProdutoResponse(Guid Id, string Nome, string Descricao, bool Ativo, List<PrecoUnidadeResponse> PrecosPorUnidade, List<ModuloResponse> Modulos);
```

```csharp
using VaultApi.Domain.Entities;
using VaultApi.Domain.Repositories;

namespace VaultApi.Application.Catalogo;

public class CatalogoService(IProdutoRepository repository)
{
    public async Task<ProdutoResponse> CriarProdutoAsync(CriarProdutoRequest request)
    {
        var produto = new Produto
        {
            Id = Guid.NewGuid(),
            Nome = request.Nome,
            Descricao = request.Descricao,
            Ativo = true,
            PrecosPorUnidade = request.PrecosPorUnidade
                .Select(p => new ProdutoPrecoUnidade { Id = Guid.NewGuid(), TipoUnidade = p.TipoUnidade, ValorAdesao = p.ValorAdesao, ValorMensalidade = p.ValorMensalidade })
                .ToList()
        };

        await repository.AddAsync(produto);
        await repository.SaveChangesAsync();
        return ToResponse(produto);
    }

    public async Task<ModuloResponse> CriarModuloAsync(Guid produtoId, CriarModuloRequest request)
    {
        var produto = await repository.GetAsync(produtoId) ?? throw new KeyNotFoundException("Produto nao encontrado.");

        var modulo = new Modulo
        {
            Id = Guid.NewGuid(),
            ProdutoId = produtoId,
            Nome = request.Nome,
            ValorAdesaoBase = request.ValorAdesaoBase,
            ValorMensalidadeBase = request.ValorMensalidadeBase,
            Ativo = true,
            Variantes = request.Variantes
                .Select(v => new ModuloVariante { Id = Guid.NewGuid(), Nome = v.Nome, TipoUnidadeAplicavel = v.TipoUnidadeAplicavel, ValorAdicionalPorUnidade = v.ValorAdicionalPorUnidade })
                .ToList()
        };
        produto.Modulos.Add(modulo);

        await repository.SaveChangesAsync();
        return new ModuloResponse(modulo.Id, modulo.Nome, modulo.ValorAdesaoBase, modulo.ValorMensalidadeBase, modulo.Ativo,
            modulo.Variantes.Select(v => new VarianteResponse(v.Id, v.Nome, v.TipoUnidadeAplicavel, v.ValorAdicionalPorUnidade)).ToList());
    }

    public async Task<ProdutoResponse?> ObterProdutoAsync(Guid id)
    {
        var produto = await repository.GetAsync(id);
        return produto is null ? null : ToResponse(produto);
    }

    private static ProdutoResponse ToResponse(Produto produto) => new(
        produto.Id, produto.Nome, produto.Descricao, produto.Ativo,
        produto.PrecosPorUnidade.Select(p => new PrecoUnidadeResponse(p.TipoUnidade, p.ValorAdesao, p.ValorMensalidade)).ToList(),
        produto.Modulos.Select(m => new ModuloResponse(m.Id, m.Nome, m.ValorAdesaoBase, m.ValorMensalidadeBase, m.Ativo,
            m.Variantes.Select(v => new VarianteResponse(v.Id, v.Nome, v.TipoUnidadeAplicavel, v.ValorAdicionalPorUnidade)).ToList())).ToList());
}
```

Register: `services.AddScoped<Application.Catalogo.CatalogoService>();`

- [ ] **Step 7: Implement controller**

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VaultApi.Api.Auth;
using VaultApi.Application.Catalogo;

namespace VaultApi.Api.Controllers;

[ApiController]
[Route("produtos")]
[Authorize]
public class CatalogoController(CatalogoService service) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = PolicyNames.RequireAdmin)]
    public async Task<ActionResult<ProdutoResponse>> CriarProduto(CriarProdutoRequest request)
    {
        var produto = await service.CriarProdutoAsync(request);
        return CreatedAtAction(nameof(Obter), new { id = produto.Id }, produto);
    }

    [HttpPost("{id:guid}/modulos")]
    [Authorize(Policy = PolicyNames.RequireAdmin)]
    public async Task<ActionResult<ModuloResponse>> CriarModulo(Guid id, CriarModuloRequest request)
    {
        var modulo = await service.CriarModuloAsync(id, request);
        return Created($"/produtos/{id}/modulos/{modulo.Id}", modulo);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = PolicyNames.RequireRevendaOrAdmin)]
    public async Task<ActionResult<ProdutoResponse>> Obter(Guid id)
    {
        var produto = await service.ObterProdutoAsync(id);
        return produto is null ? NotFound() : Ok(produto);
    }
}
```

- [ ] **Step 8: Generate the migration**

```bash
dotnet ef migrations add AddCatalogo \
  --project src/VaultApi.Infrastructure \
  --startup-project src/VaultApi.Api \
  --output-dir Migrations
```

- [ ] **Step 9: Run test to verify it passes**

Run: `dotnet test tests/VaultApi.Tests --filter CatalogoEndpointTests`
Expected: PASS.

- [ ] **Step 10: Commit**

```bash
git add -A
git commit -m "feat: add catalog entities (Produto/ProdutoPrecoUnidade/Modulo/ModuloVariante)"
```

---

## Task 10: Catalog Price History

**Files:**
- Create: `src/VaultApi.Domain/Entities/HistoricoPrecoCatalogo.cs`
- Create: `src/VaultApi.Infrastructure/Persistence/Configurations/HistoricoPrecoCatalogoConfiguration.cs`
- Modify: `src/VaultApi.Application/Catalogo/CatalogoService.cs` (add `AtualizarPrecoUnidadeAsync` + history logging)
- Create: `src/VaultApi.Domain/Repositories/IHistoricoPrecoCatalogoRepository.cs`
- Create: `src/VaultApi.Infrastructure/Repositories/HistoricoPrecoCatalogoRepository.cs`
- Modify: `src/VaultApi.Infrastructure/DependencyInjection.cs`
- Create: `src/VaultApi.Api/Controllers/CatalogoController.cs` (add `PATCH` endpoint — same file, Modify)
- Create migration: `src/VaultApi.Infrastructure/Migrations/*_AddHistoricoPrecoCatalogo.cs`
- Test: `tests/VaultApi.Tests/Catalogo/HistoricoPrecoTests.cs`

**Interfaces:**
- Consumes: `ProdutoPrecoUnidade`, `CatalogoService` (Task 9), `ICurrentUser` (Task 8).
- Produces: `HistoricoPrecoCatalogo(Id, EntidadeTipo, EntidadeId, TipoValor, ValorAnterior, ValorNovo, DataAlteracao, UsuarioId)`, `CatalogoService.AtualizarPrecoUnidadeAsync(Guid produtoId, TipoUnidade tipo, decimal novoValorAdesao, decimal novoValorMensalidade, Guid usuarioId)`.

- [ ] **Step 1: Write the failing test**

```csharp
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using VaultApi.Domain.Entities;
using VaultApi.Domain.Enums;
using VaultApi.Infrastructure.Persistence;
using VaultApi.Tests.Fixtures;

namespace VaultApi.Tests.Catalogo;

[Collection("Postgres")]
public class HistoricoPrecoTests(PostgresFixture postgres)
{
    [Fact]
    public async Task Updating_produto_preco_unidade_appends_history_row()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(postgres.ConnectionString).UseSnakeCaseNamingConvention().Options;
        await using (var db = new AppDbContext(options))
        {
            await db.Database.EnsureDeletedAsync();
            await db.Database.MigrateAsync();
        }

        var produtoId = Guid.NewGuid();
        var precoId = Guid.NewGuid();
        await using (var db = new AppDbContext(options))
        {
            db.Set<Produto>().Add(new Produto { Id = produtoId, Nome = "P", Descricao = "D", Ativo = true });
            db.Set<ProdutoPrecoUnidade>().Add(new ProdutoPrecoUnidade
            {
                Id = precoId, ProdutoId = produtoId, TipoUnidade = TipoUnidade.PDV, ValorAdesao = 50m, ValorMensalidade = 20m
            });
            await db.SaveChangesAsync();
        }

        var usuarioId = Guid.NewGuid();
        await using (var scopeDb = new AppDbContext(options))
        {
            var repository = new VaultApi.Infrastructure.Repositories.ProdutoRepository(scopeDb);
            var historicoRepository = new VaultApi.Infrastructure.Repositories.HistoricoPrecoCatalogoRepository(scopeDb);
            var service = new VaultApi.Application.Catalogo.CatalogoService(repository, historicoRepository);

            await service.AtualizarPrecoUnidadeAsync(produtoId, TipoUnidade.PDV, novoValorAdesao: 60m, novoValorMensalidade: 25m, usuarioId);
        }

        await using var db2 = new AppDbContext(options);
        var historico = await db2.Set<HistoricoPrecoCatalogo>().ToListAsync();
        historico.Should().HaveCount(2);
        historico.Should().Contain(h => h.TipoValor == TipoValorCatalogo.Mensalidade && h.ValorAnterior == 20m && h.ValorNovo == 25m);
        historico.Should().Contain(h => h.TipoValor == TipoValorCatalogo.Adesao && h.ValorAnterior == 50m && h.ValorNovo == 60m);

        var precoAtualizado = await db2.Set<ProdutoPrecoUnidade>().SingleAsync(p => p.Id == precoId);
        precoAtualizado.ValorAdesao.Should().Be(60m);
        precoAtualizado.ValorMensalidade.Should().Be(25m);
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test tests/VaultApi.Tests --filter HistoricoPrecoTests`
Expected: FAIL to compile — `HistoricoPrecoCatalogo`, `HistoricoPrecoCatalogoRepository`, `AtualizarPrecoUnidadeAsync` don't exist.

- [ ] **Step 3: Implement the entity, configuration, repository**

```csharp
using VaultApi.Domain.Enums;

namespace VaultApi.Domain.Entities;

public class HistoricoPrecoCatalogo
{
    public Guid Id { get; set; }
    public EntidadeTipoCatalogo EntidadeTipo { get; set; }
    public Guid EntidadeId { get; set; }
    public TipoValorCatalogo TipoValor { get; set; }
    public decimal ValorAnterior { get; set; }
    public decimal ValorNovo { get; set; }
    public DateTimeOffset DataAlteracao { get; set; }
    public Guid UsuarioId { get; set; }
}
```

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VaultApi.Domain.Entities;

namespace VaultApi.Infrastructure.Persistence.Configurations;

public class HistoricoPrecoCatalogoConfiguration : IEntityTypeConfiguration<HistoricoPrecoCatalogo>
{
    public void Configure(EntityTypeBuilder<HistoricoPrecoCatalogo> builder)
    {
        builder.HasKey(h => h.Id);
        builder.Property(h => h.EntidadeTipo).HasConversion<string>().HasMaxLength(30);
        builder.Property(h => h.TipoValor).HasConversion<string>().HasMaxLength(30);
    }
}
```

```csharp
using VaultApi.Domain.Entities;

namespace VaultApi.Domain.Repositories;

public interface IHistoricoPrecoCatalogoRepository
{
    Task AddAsync(HistoricoPrecoCatalogo registro);
}
```

```csharp
using VaultApi.Domain.Entities;
using VaultApi.Domain.Repositories;
using VaultApi.Infrastructure.Persistence;

namespace VaultApi.Infrastructure.Repositories;

public class HistoricoPrecoCatalogoRepository(AppDbContext db) : IHistoricoPrecoCatalogoRepository
{
    public async Task AddAsync(HistoricoPrecoCatalogo registro) => await db.Set<HistoricoPrecoCatalogo>().AddAsync(registro);
}
```

Register: `services.AddScoped<Domain.Repositories.IHistoricoPrecoCatalogoRepository, Repositories.HistoricoPrecoCatalogoRepository>();`

- [ ] **Step 4: Implement `AtualizarPrecoUnidadeAsync` in `CatalogoService`**

Change the `CatalogoService` primary constructor to `CatalogoService(IProdutoRepository repository, IHistoricoPrecoCatalogoRepository historicoRepository)` and add:

```csharp
public async Task AtualizarPrecoUnidadeAsync(Guid produtoId, TipoUnidade tipoUnidade, decimal novoValorAdesao, decimal novoValorMensalidade, Guid usuarioId)
{
    var produto = await repository.GetAsync(produtoId) ?? throw new KeyNotFoundException("Produto nao encontrado.");
    var preco = produto.PrecosPorUnidade.SingleOrDefault(p => p.TipoUnidade == tipoUnidade)
        ?? throw new KeyNotFoundException("Preco por unidade nao encontrado para este produto.");

    var agora = DateTimeOffset.UtcNow;

    if (preco.ValorAdesao != novoValorAdesao)
    {
        await historicoRepository.AddAsync(new HistoricoPrecoCatalogo
        {
            Id = Guid.NewGuid(), EntidadeTipo = EntidadeTipoCatalogo.ProdutoPrecoUnidade, EntidadeId = preco.Id,
            TipoValor = TipoValorCatalogo.Adesao, ValorAnterior = preco.ValorAdesao, ValorNovo = novoValorAdesao,
            DataAlteracao = agora, UsuarioId = usuarioId
        });
        preco.ValorAdesao = novoValorAdesao;
    }

    if (preco.ValorMensalidade != novoValorMensalidade)
    {
        await historicoRepository.AddAsync(new HistoricoPrecoCatalogo
        {
            Id = Guid.NewGuid(), EntidadeTipo = EntidadeTipoCatalogo.ProdutoPrecoUnidade, EntidadeId = preco.Id,
            TipoValor = TipoValorCatalogo.Mensalidade, ValorAnterior = preco.ValorMensalidade, ValorNovo = novoValorMensalidade,
            DataAlteracao = agora, UsuarioId = usuarioId
        });
        preco.ValorMensalidade = novoValorMensalidade;
    }

    await repository.SaveChangesAsync();
}
```

Add `using VaultApi.Domain.Enums;` to `CatalogoService.cs`.

- [ ] **Step 5: Add the `PATCH` endpoint**

Add to `CatalogoController` (inject `ICurrentUser` via constructor: `CatalogoController(CatalogoService service, ICurrentUser currentUser)`):

```csharp
public record AtualizarPrecoUnidadeRequest(VaultApi.Domain.Enums.TipoUnidade TipoUnidade, decimal ValorAdesao, decimal ValorMensalidade);

[HttpPatch("{id:guid}/precos")]
[Authorize(Policy = PolicyNames.RequireAdmin)]
public async Task<IActionResult> AtualizarPreco(Guid id, AtualizarPrecoUnidadeRequest request)
{
    await service.AtualizarPrecoUnidadeAsync(id, request.TipoUnidade, request.ValorAdesao, request.ValorMensalidade,
        Guid.Parse(User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)!.Value));
    return NoContent();
}
```

Add `using VaultApi.Application.Abstractions;` to the controller file for `ICurrentUser`.

- [ ] **Step 6: Generate the migration**

```bash
dotnet ef migrations add AddHistoricoPrecoCatalogo \
  --project src/VaultApi.Infrastructure \
  --startup-project src/VaultApi.Api \
  --output-dir Migrations
```

- [ ] **Step 7: Run test to verify it passes**

Run: `dotnet test tests/VaultApi.Tests --filter HistoricoPrecoTests`
Expected: PASS.

- [ ] **Step 8: Run full suite and commit**

Run: `dotnet test`
Expected: all tests PASS.

```bash
git add -A
git commit -m "feat: log catalog price changes to HistoricoPrecoCatalogo"
```

---

## Task 11: Contrato Aggregate Entities

**Files:**
- Create: `src/VaultApi.Domain/Entities/Contrato.cs`, `ContratoItem.cs`, `ContratoItemUnidade.cs`, `ContratoItemModulo.cs`
- Create: `src/VaultApi.Domain/Repositories/IContratoRepository.cs`
- Create: `src/VaultApi.Infrastructure/Persistence/Configurations/ContratoConfiguration.cs`, `ContratoItemConfiguration.cs`, `ContratoItemUnidadeConfiguration.cs`, `ContratoItemModuloConfiguration.cs`
- Create: `src/VaultApi.Infrastructure/Repositories/ContratoRepository.cs`
- Modify: `src/VaultApi.Infrastructure/DependencyInjection.cs`
- Create migration: `src/VaultApi.Infrastructure/Migrations/*_AddContrato.cs`
- Test: `tests/VaultApi.Tests/Contratos/ContratoPersistenceTests.cs`

**Interfaces:**
- Consumes: `Cliente` (Task 7), `Produto`/`Modulo`/`ModuloVariante` (Task 9).
- Produces: `Contrato(Id, ClienteId, RevendaId?, Ativo, DataInicio, DataFim?, Itens)`, `ContratoItem(Id, ContratoId, ProdutoId, ValorAdesaoOverride?, ValorMensalidadeOverride?, TipoDesconto?, ValorDesconto?, Unidades, Modulos)`, `ContratoItemUnidade(Id, ContratoItemId, TipoUnidade, Quantidade)`, `ContratoItemModulo(Id, ContratoItemId, ModuloId, ModuloVarianteId?, Ativo, ValorOverride?)`, `IContratoRepository { AddAsync, GetAsync (fully included), ListAsync(bool semRestricao, Guid? revendaId), SaveChangesAsync }` — Task 12's `PricingResolver` input is built from a fully-loaded `Contrato`, and Task 13's controller consumes `IContratoRepository` directly.

- [ ] **Step 1: Write the failing persistence test**

```csharp
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using VaultApi.Domain.Entities;
using VaultApi.Domain.Enums;
using VaultApi.Infrastructure.Persistence;
using VaultApi.Tests.Fixtures;

namespace VaultApi.Tests.Contratos;

[Collection("Postgres")]
public class ContratoPersistenceTests(PostgresFixture postgres)
{
    [Fact]
    public async Task Contrato_roundtrips_with_itens_unidades_and_modulos()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(postgres.ConnectionString).UseSnakeCaseNamingConvention().Options;
        await using (var db = new AppDbContext(options))
        {
            await db.Database.EnsureDeletedAsync();
            await db.Database.MigrateAsync();
        }

        var clienteId = Guid.NewGuid();
        var produtoId = Guid.NewGuid();
        var moduloId = Guid.NewGuid();
        await using (var db = new AppDbContext(options))
        {
            db.Set<Cliente>().Add(new Cliente { Id = clienteId, Nome = "Cliente", Cnpj = "1", RevendaId = null });
            db.Set<Produto>().Add(new Produto { Id = produtoId, Nome = "P", Descricao = "D", Ativo = true });
            db.Set<Modulo>().Add(new Modulo { Id = moduloId, ProdutoId = produtoId, Nome = "TEF", Ativo = true });
            await db.SaveChangesAsync();
        }

        var contratoId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        await using (var db = new AppDbContext(options))
        {
            db.Set<Contrato>().Add(new Contrato
            {
                Id = contratoId, ClienteId = clienteId, RevendaId = null, Ativo = true, DataInicio = DateOnly.FromDateTime(DateTime.UtcNow),
                Itens =
                [
                    new ContratoItem
                    {
                        Id = itemId, ContratoId = contratoId, ProdutoId = produtoId,
                        Unidades = [ new ContratoItemUnidade { Id = Guid.NewGuid(), ContratoItemId = itemId, TipoUnidade = TipoUnidade.PDV, Quantidade = 5 } ],
                        Modulos = [ new ContratoItemModulo { Id = Guid.NewGuid(), ContratoItemId = itemId, ModuloId = moduloId, Ativo = true } ]
                    }
                ]
            });
            await db.SaveChangesAsync();
        }

        await using var readDb = new AppDbContext(options);
        var contrato = await readDb.Set<Contrato>()
            .Include(c => c.Itens).ThenInclude(i => i.Unidades)
            .Include(c => c.Itens).ThenInclude(i => i.Modulos)
            .SingleAsync(c => c.Id == contratoId);

        contrato.Itens.Should().ContainSingle();
        contrato.Itens[0].Unidades.Should().ContainSingle(u => u.TipoUnidade == TipoUnidade.PDV && u.Quantidade == 5);
        contrato.Itens[0].Modulos.Should().ContainSingle(m => m.ModuloId == moduloId && m.Ativo);
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test tests/VaultApi.Tests --filter ContratoPersistenceTests`
Expected: FAIL to compile — `Contrato` and related entities don't exist.

- [ ] **Step 3: Implement the entities**

```csharp
namespace VaultApi.Domain.Entities;

public class Contrato
{
    public Guid Id { get; set; }
    public Guid ClienteId { get; set; }
    public Guid? RevendaId { get; set; }
    public bool Ativo { get; set; } = true;
    public DateOnly DataInicio { get; set; }
    public DateOnly? DataFim { get; set; }
    public List<ContratoItem> Itens { get; set; } = [];
}
```

```csharp
using VaultApi.Domain.Enums;

namespace VaultApi.Domain.Entities;

public class ContratoItem
{
    public Guid Id { get; set; }
    public Guid ContratoId { get; set; }
    public Guid ProdutoId { get; set; }
    public decimal? ValorAdesaoOverride { get; set; }
    public decimal? ValorMensalidadeOverride { get; set; }
    public TipoDesconto? TipoDesconto { get; set; }
    public decimal? ValorDesconto { get; set; }
    public List<ContratoItemUnidade> Unidades { get; set; } = [];
    public List<ContratoItemModulo> Modulos { get; set; } = [];
}
```

```csharp
using VaultApi.Domain.Enums;

namespace VaultApi.Domain.Entities;

public class ContratoItemUnidade
{
    public Guid Id { get; set; }
    public Guid ContratoItemId { get; set; }
    public TipoUnidade TipoUnidade { get; set; }
    public int Quantidade { get; set; }
}
```

```csharp
namespace VaultApi.Domain.Entities;

public class ContratoItemModulo
{
    public Guid Id { get; set; }
    public Guid ContratoItemId { get; set; }
    public Guid ModuloId { get; set; }
    public Guid? ModuloVarianteId { get; set; }
    public bool Ativo { get; set; } = true;
    public decimal? ValorOverride { get; set; }
}
```

- [ ] **Step 4: Implement EF configurations**

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VaultApi.Domain.Entities;

namespace VaultApi.Infrastructure.Persistence.Configurations;

public class ContratoConfiguration : IEntityTypeConfiguration<Contrato>
{
    public void Configure(EntityTypeBuilder<Contrato> builder)
    {
        builder.HasKey(c => c.Id);
        builder.HasMany(c => c.Itens).WithOne().HasForeignKey(i => i.ContratoId);
    }
}
```

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VaultApi.Domain.Entities;

namespace VaultApi.Infrastructure.Persistence.Configurations;

public class ContratoItemConfiguration : IEntityTypeConfiguration<ContratoItem>
{
    public void Configure(EntityTypeBuilder<ContratoItem> builder)
    {
        builder.HasKey(i => i.Id);
        builder.Property(i => i.TipoDesconto).HasConversion<string>().HasMaxLength(20);
        builder.HasMany(i => i.Unidades).WithOne().HasForeignKey(u => u.ContratoItemId);
        builder.HasMany(i => i.Modulos).WithOne().HasForeignKey(m => m.ContratoItemId);
    }
}
```

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VaultApi.Domain.Entities;

namespace VaultApi.Infrastructure.Persistence.Configurations;

public class ContratoItemUnidadeConfiguration : IEntityTypeConfiguration<ContratoItemUnidade>
{
    public void Configure(EntityTypeBuilder<ContratoItemUnidade> builder)
    {
        builder.HasKey(u => u.Id);
        builder.Property(u => u.TipoUnidade).HasConversion<string>().HasMaxLength(20);
        builder.HasIndex(u => new { u.ContratoItemId, u.TipoUnidade }).IsUnique();
    }
}
```

```csharp
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
```

- [ ] **Step 5: Implement repository**

```csharp
using VaultApi.Domain.Entities;

namespace VaultApi.Domain.Repositories;

public interface IContratoRepository
{
    Task AddAsync(Contrato contrato);
    Task<Contrato?> GetAsync(Guid id);
    Task<List<Contrato>> ListAsync(bool semRestricao, Guid? revendaId);
    Task SaveChangesAsync();
}
```

```csharp
using Microsoft.EntityFrameworkCore;
using VaultApi.Domain.Entities;
using VaultApi.Domain.Repositories;
using VaultApi.Infrastructure.Persistence;

namespace VaultApi.Infrastructure.Repositories;

public class ContratoRepository(AppDbContext db) : IContratoRepository
{
    public async Task AddAsync(Contrato contrato) => await db.Set<Contrato>().AddAsync(contrato);

    public Task<Contrato?> GetAsync(Guid id) => Query().SingleOrDefaultAsync(c => c.Id == id);

    public Task<List<Contrato>> ListAsync(bool semRestricao, Guid? revendaId) => Query()
        .Where(c => semRestricao || c.RevendaId == revendaId)
        .ToListAsync();

    public Task SaveChangesAsync() => db.SaveChangesAsync();

    private IQueryable<Contrato> Query() => db.Set<Contrato>()
        .Include(c => c.Itens).ThenInclude(i => i.Unidades)
        .Include(c => c.Itens).ThenInclude(i => i.Modulos);
}
```

Register: `services.AddScoped<Domain.Repositories.IContratoRepository, Repositories.ContratoRepository>();`

- [ ] **Step 6: Generate the migration**

```bash
dotnet ef migrations add AddContrato \
  --project src/VaultApi.Infrastructure \
  --startup-project src/VaultApi.Api \
  --output-dir Migrations
```

- [ ] **Step 7: Run test to verify it passes**

Run: `dotnet test tests/VaultApi.Tests --filter ContratoPersistenceTests`
Expected: PASS.

- [ ] **Step 8: Commit**

```bash
git add -A
git commit -m "feat: add Contrato aggregate entities (ContratoItem/Unidade/Modulo)"
```

---

## Task 12: PricingResolver (pure pricing logic)

**Files:**
- Create: `src/VaultApi.Application/Pricing/ItemPricingInput.cs`
- Create: `src/VaultApi.Application/Pricing/ItemPricingResult.cs`
- Create: `src/VaultApi.Application/Pricing/PricingResolver.cs`
- Test: `tests/VaultApi.Tests/Pricing/PricingResolverTests.cs`

**Interfaces:**
- Consumes: nothing (pure, plain record inputs mirroring the shapes from Task 9/11 — no direct dependency on those entities, to keep this fully unit-testable).
- Produces: `PricingResolver.Resolver(ItemPricingInput) : ItemPricingResult { ValorAdesaoTotal, ValorMensalidadeTotal }` — Task 13's `ContratoService` builds `ItemPricingInput` from loaded `Contrato`/`Produto` entities and calls this.

- [ ] **Step 1: Write the failing unit tests**

```csharp
using FluentAssertions;
using VaultApi.Application.Pricing;
using VaultApi.Domain.Enums;

namespace VaultApi.Tests.Pricing;

public class PricingResolverTests
{
    [Fact]
    public void Calculates_base_price_from_unit_quantities_times_unit_price()
    {
        var input = new ItemPricingInput(
            PrecosPorUnidade: new Dictionary<TipoUnidade, (decimal Adesao, decimal Mensalidade)>
            {
                [TipoUnidade.Servidor] = (500m, 100m),
                [TipoUnidade.PDV] = (50m, 20m)
            },
            Quantidades: new Dictionary<TipoUnidade, int> { [TipoUnidade.Servidor] = 1, [TipoUnidade.PDV] = 5 },
            Modulos: [],
            ValorAdesaoOverride: null,
            ValorMensalidadeOverride: null,
            TipoDesconto: null,
            ValorDesconto: null);

        var result = PricingResolver.Resolver(input);

        // 1 servidor (500 adesao / 100 mensal) + 5 pdv (50 adesao / 20 mensal each)
        result.ValorAdesaoTotal.Should().Be(500m + 5 * 50m);
        result.ValorMensalidadeTotal.Should().Be(100m + 5 * 20m);
    }

    [Fact]
    public void Adds_flat_modulo_price_and_variante_price_per_applicable_unit()
    {
        var input = new ItemPricingInput(
            PrecosPorUnidade: new Dictionary<TipoUnidade, (decimal, decimal)> { [TipoUnidade.PDV] = (0m, 0m) },
            Quantidades: new Dictionary<TipoUnidade, int> { [TipoUnidade.PDV] = 5 },
            Modulos:
            [
                new ModuloPricingInput(Ativo: true, ValorAdesaoBase: 200m, ValorMensalidadeBase: 30m,
                    VarianteTipoUnidadeAplicavel: TipoUnidade.PDV, ValorAdicionalPorUnidade: 15m, ValorOverride: null)
            ],
            ValorAdesaoOverride: null, ValorMensalidadeOverride: null, TipoDesconto: null, ValorDesconto: null);

        var result = PricingResolver.Resolver(input);

        // modulo flat (200/30) + variante 15 x 5 pdv = 75 additional monthly-side charge only (adesao has no variante split here)
        result.ValorAdesaoTotal.Should().Be(200m);
        result.ValorMensalidadeTotal.Should().Be(30m + 5 * 15m);
    }

    [Fact]
    public void Ignores_inactive_modulo()
    {
        var input = new ItemPricingInput(
            PrecosPorUnidade: new Dictionary<TipoUnidade, (decimal, decimal)>(),
            Quantidades: new Dictionary<TipoUnidade, int>(),
            Modulos: [ new ModuloPricingInput(Ativo: false, ValorAdesaoBase: 999m, ValorMensalidadeBase: 999m,
                VarianteTipoUnidadeAplicavel: null, ValorAdicionalPorUnidade: null, ValorOverride: null) ],
            ValorAdesaoOverride: null, ValorMensalidadeOverride: null, TipoDesconto: null, ValorDesconto: null);

        var result = PricingResolver.Resolver(input);

        result.ValorAdesaoTotal.Should().Be(0m);
        result.ValorMensalidadeTotal.Should().Be(0m);
    }

    [Fact]
    public void Fixo_override_replaces_calculated_total()
    {
        var input = new ItemPricingInput(
            PrecosPorUnidade: new Dictionary<TipoUnidade, (decimal, decimal)> { [TipoUnidade.PDV] = (50m, 20m) },
            Quantidades: new Dictionary<TipoUnidade, int> { [TipoUnidade.PDV] = 10 },
            Modulos: [],
            ValorAdesaoOverride: 300m, ValorMensalidadeOverride: 120m,
            TipoDesconto: TipoDesconto.Fixo, ValorDesconto: null);

        var result = PricingResolver.Resolver(input);

        result.ValorAdesaoTotal.Should().Be(300m);
        result.ValorMensalidadeTotal.Should().Be(120m);
    }

    [Fact]
    public void Percentual_discount_applies_over_calculated_base()
    {
        var input = new ItemPricingInput(
            PrecosPorUnidade: new Dictionary<TipoUnidade, (decimal, decimal)> { [TipoUnidade.PDV] = (100m, 40m) },
            Quantidades: new Dictionary<TipoUnidade, int> { [TipoUnidade.PDV] = 1 },
            Modulos: [],
            ValorAdesaoOverride: null, ValorMensalidadeOverride: null,
            TipoDesconto: TipoDesconto.Percentual, ValorDesconto: 10m);

        var result = PricingResolver.Resolver(input);

        result.ValorAdesaoTotal.Should().Be(90m);
        result.ValorMensalidadeTotal.Should().Be(36m);
    }

    [Fact]
    public void Valor_discount_subtracts_fixed_amount_from_base()
    {
        var input = new ItemPricingInput(
            PrecosPorUnidade: new Dictionary<TipoUnidade, (decimal, decimal)> { [TipoUnidade.PDV] = (100m, 40m) },
            Quantidades: new Dictionary<TipoUnidade, int> { [TipoUnidade.PDV] = 1 },
            Modulos: [],
            ValorAdesaoOverride: null, ValorMensalidadeOverride: null,
            TipoDesconto: TipoDesconto.Valor, ValorDesconto: 15m);

        var result = PricingResolver.Resolver(input);

        result.ValorAdesaoTotal.Should().Be(85m);
        result.ValorMensalidadeTotal.Should().Be(25m);
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test tests/VaultApi.Tests --filter PricingResolverTests`
Expected: FAIL to compile — `ItemPricingInput`, `ModuloPricingInput`, `ItemPricingResult`, `PricingResolver` don't exist.

- [ ] **Step 3: Implement the pricing types and resolver**

```csharp
using VaultApi.Domain.Enums;

namespace VaultApi.Application.Pricing;

public record ModuloPricingInput(
    bool Ativo,
    decimal? ValorAdesaoBase,
    decimal? ValorMensalidadeBase,
    TipoUnidade? VarianteTipoUnidadeAplicavel,
    decimal? ValorAdicionalPorUnidade,
    decimal? ValorOverride);

public record ItemPricingInput(
    IReadOnlyDictionary<TipoUnidade, (decimal Adesao, decimal Mensalidade)> PrecosPorUnidade,
    IReadOnlyDictionary<TipoUnidade, int> Quantidades,
    IReadOnlyList<ModuloPricingInput> Modulos,
    decimal? ValorAdesaoOverride,
    decimal? ValorMensalidadeOverride,
    TipoDesconto? TipoDesconto,
    decimal? ValorDesconto);
```

```csharp
namespace VaultApi.Application.Pricing;

public record ItemPricingResult(decimal ValorAdesaoTotal, decimal ValorMensalidadeTotal);
```

```csharp
using VaultApi.Domain.Enums;

namespace VaultApi.Application.Pricing;

public static class PricingResolver
{
    public static ItemPricingResult Resolver(ItemPricingInput input)
    {
        var (adesaoBase, mensalidadeBase) = CalcularBaseProduto(input);
        var (adesaoModulos, mensalidadeModulos) = CalcularBaseModulos(input);

        var adesaoTotal = adesaoBase + adesaoModulos;
        var mensalidadeTotal = mensalidadeBase + mensalidadeModulos;

        if (input.TipoDesconto == Domain.Enums.TipoDesconto.Fixo)
        {
            return new ItemPricingResult(input.ValorAdesaoOverride ?? adesaoTotal, input.ValorMensalidadeOverride ?? mensalidadeTotal);
        }

        if (input.TipoDesconto == Domain.Enums.TipoDesconto.Percentual && input.ValorDesconto is { } percentual)
        {
            var fator = 1 - percentual / 100m;
            return new ItemPricingResult(adesaoTotal * fator, mensalidadeTotal * fator);
        }

        if (input.TipoDesconto == Domain.Enums.TipoDesconto.Valor && input.ValorDesconto is { } valor)
        {
            return new ItemPricingResult(adesaoTotal - valor, mensalidadeTotal - valor);
        }

        return new ItemPricingResult(adesaoTotal, mensalidadeTotal);
    }

    private static (decimal Adesao, decimal Mensalidade) CalcularBaseProduto(ItemPricingInput input)
    {
        decimal adesao = 0, mensalidade = 0;
        foreach (var (tipoUnidade, quantidade) in input.Quantidades)
        {
            if (!input.PrecosPorUnidade.TryGetValue(tipoUnidade, out var preco))
            {
                continue;
            }

            adesao += preco.Adesao * quantidade;
            mensalidade += preco.Mensalidade * quantidade;
        }

        return (adesao, mensalidade);
    }

    private static (decimal Adesao, decimal Mensalidade) CalcularBaseModulos(ItemPricingInput input)
    {
        decimal adesao = 0, mensalidade = 0;
        foreach (var modulo in input.Modulos.Where(m => m.Ativo))
        {
            adesao += modulo.ValorAdesaoBase ?? 0m;
            mensalidade += modulo.ValorMensalidadeBase ?? 0m;

            if (modulo.VarianteTipoUnidadeAplicavel is { } tipoUnidade
                && modulo.ValorAdicionalPorUnidade is { } valorAdicional
                && input.Quantidades.TryGetValue(tipoUnidade, out var quantidade))
            {
                mensalidade += valorAdicional * quantidade;
            }
        }

        return (adesao, mensalidade);
    }
}
```

- [ ] **Step 4: Run tests to verify they pass**

Run: `dotnet test tests/VaultApi.Tests --filter PricingResolverTests`
Expected: PASS (all 6 tests).

- [ ] **Step 5: Commit**

```bash
git add -A
git commit -m "feat: add pure PricingResolver for contract item pricing"
```

---

## Task 13: Contrato Endpoints (Admin write, price-hiding read split)

**Files:**
- Create: `src/VaultApi.Application/Contratos/ContratoDtos.cs`
- Create: `src/VaultApi.Application/Contratos/ContratoService.cs`
- Create: `src/VaultApi.Api/Controllers/ContratosController.cs`
- Modify: `src/VaultApi.Infrastructure/DependencyInjection.cs`
- Test: `tests/VaultApi.Tests/Contratos/ContratoEndpointTests.cs`

**Interfaces:**
- Consumes: `IContratoRepository` (Task 11), `IProdutoRepository` (Task 9), `IScopeFilter` (Task 8), `PricingResolver` (Task 12).
- Produces: `ContratoAdminResponse` (includes prices/overrides), `ContratoPublicoResponse` (no price fields), `ContratoService.CriarAsync`, `ContratoService.ListarAdminAsync(ScopeResult)`, `ContratoService.ListarPublicoAsync(ScopeResult)` — Task 14's `LicencaService` consumes `IContratoRepository.GetAsync` the same way this task does.

- [ ] **Step 1: Write the failing integration test**

```csharp
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using VaultApi.Domain.Entities;
using VaultApi.Domain.Enums;
using VaultApi.Infrastructure.Persistence;
using VaultApi.Tests.Fixtures;

namespace VaultApi.Tests.Contratos;

[Collection("Postgres")]
public class ContratoEndpointTests(PostgresFixture postgres)
{
    [Fact]
    public async Task Admin_sees_prices_revenda_does_not()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(postgres.ConnectionString).UseSnakeCaseNamingConvention().Options;
        await using (var db = new AppDbContext(options))
        {
            await db.Database.EnsureDeletedAsync();
            await db.Database.MigrateAsync();
        }

        var revendaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var produtoId = Guid.NewGuid();
        await using (var db = new AppDbContext(options))
        {
            db.Set<Revenda>().Add(new Revenda { Id = revendaId, Nome = "R", Cnpj = "1", Ativo = true });
            db.Set<Cliente>().Add(new Cliente { Id = clienteId, Nome = "C", Cnpj = "2", RevendaId = revendaId });
            db.Set<Produto>().Add(new Produto
            {
                Id = produtoId, Nome = "P", Descricao = "D", Ativo = true,
                PrecosPorUnidade = [ new ProdutoPrecoUnidade { Id = Guid.NewGuid(), ProdutoId = produtoId, TipoUnidade = TipoUnidade.PDV, ValorAdesao = 50m, ValorMensalidade = 20m } ]
            });
            await db.SaveChangesAsync();
        }

        await using var factory = new ApiFactory(postgres.ConnectionString);
        var adminToken = await TestAuth.CreateUserAndLoginAsync(factory, Nivel.Admin, revendaId: null);
        using var adminClient = factory.CreateClient();
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var criarResponse = await adminClient.PostAsJsonAsync("/contratos", new
        {
            clienteId, revendaId, dataInicio = DateOnly.FromDateTime(DateTime.UtcNow),
            itens = new[]
            {
                new
                {
                    produtoId,
                    unidades = new[] { new { tipoUnidade = "PDV", quantidade = 3 } },
                    modulos = Array.Empty<object>(),
                    valorAdesaoOverride = (decimal?)null, valorMensalidadeOverride = (decimal?)null,
                    tipoDesconto = (string?)null, valorDesconto = (decimal?)null
                }
            }
        });
        criarResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var contrato = await criarResponse.Content.ReadFromJsonAsync<ContratoAdminDto>();

        var adminGet = await adminClient.GetFromJsonAsync<ContratoAdminDto>($"/contratos/{contrato!.Id}");
        adminGet!.Itens[0].ValorMensalidadeCalculado.Should().Be(60m); // 3 pdv x 20

        var revendaToken = await TestAuth.CreateUserAndLoginAsync(factory, Nivel.Revenda, revendaId);
        using var revendaClient = factory.CreateClient();
        revendaClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", revendaToken);

        var revendaGetResponse = await revendaClient.GetAsync($"/contratos/{contrato.Id}");
        revendaGetResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await revendaGetResponse.Content.ReadAsStringAsync();
        body.Should().NotContain("valorMensalidadeCalculado", "revenda nunca deve ver valores negociados");
        body.Should().Contain("produtoId");
    }

    private record ContratoAdminDto(Guid Id, List<ContratoItemAdminDto> Itens);
    private record ContratoItemAdminDto(Guid ProdutoId, decimal ValorAdesaoCalculado, decimal ValorMensalidadeCalculado);
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test tests/VaultApi.Tests --filter ContratoEndpointTests`
Expected: FAIL — `/contratos` endpoints don't exist.

- [ ] **Step 3: Implement DTOs**

```csharp
using VaultApi.Domain.Enums;

namespace VaultApi.Application.Contratos;

public record UnidadeRequest(TipoUnidade TipoUnidade, int Quantidade);
public record ModuloItemRequest(Guid ModuloId, Guid? ModuloVarianteId, bool Ativo, decimal? ValorOverride);
public record ContratoItemRequest(
    Guid ProdutoId,
    List<UnidadeRequest> Unidades,
    List<ModuloItemRequest> Modulos,
    decimal? ValorAdesaoOverride,
    decimal? ValorMensalidadeOverride,
    TipoDesconto? TipoDesconto,
    decimal? ValorDesconto);
public record CriarContratoRequest(Guid ClienteId, Guid? RevendaId, DateOnly DataInicio, List<ContratoItemRequest> Itens);

public record ContratoItemAdminResponse(
    Guid Id, Guid ProdutoId, List<UnidadeRequest> Unidades,
    decimal ValorAdesaoCalculado, decimal ValorMensalidadeCalculado,
    decimal? ValorAdesaoOverride, decimal? ValorMensalidadeOverride, TipoDesconto? TipoDesconto, decimal? ValorDesconto);
public record ContratoAdminResponse(Guid Id, Guid ClienteId, Guid? RevendaId, bool Ativo, DateOnly DataInicio, DateOnly? DataFim, List<ContratoItemAdminResponse> Itens);

public record ContratoItemPublicoResponse(Guid Id, Guid ProdutoId, List<UnidadeRequest> Unidades, List<Guid> ModulosAtivos);
public record ContratoPublicoResponse(Guid Id, Guid ClienteId, Guid? RevendaId, bool Ativo, DateOnly DataInicio, DateOnly? DataFim, List<ContratoItemPublicoResponse> Itens);
```

- [ ] **Step 4: Implement `ContratoService`**

```csharp
using VaultApi.Application.Abstractions;
using VaultApi.Application.Pricing;
using VaultApi.Domain.Entities;
using VaultApi.Domain.Repositories;

namespace VaultApi.Application.Contratos;

public class ContratoService(IContratoRepository contratoRepository, IProdutoRepository produtoRepository)
{
    public async Task<ContratoAdminResponse> CriarAsync(CriarContratoRequest request)
    {
        var contratoId = Guid.NewGuid();
        var itens = new List<ContratoItem>();

        foreach (var itemRequest in request.Itens)
        {
            var itemId = Guid.NewGuid();
            itens.Add(new ContratoItem
            {
                Id = itemId,
                ContratoId = contratoId,
                ProdutoId = itemRequest.ProdutoId,
                ValorAdesaoOverride = itemRequest.ValorAdesaoOverride,
                ValorMensalidadeOverride = itemRequest.ValorMensalidadeOverride,
                TipoDesconto = itemRequest.TipoDesconto,
                ValorDesconto = itemRequest.ValorDesconto,
                Unidades = itemRequest.Unidades.Select(u => new ContratoItemUnidade { Id = Guid.NewGuid(), ContratoItemId = itemId, TipoUnidade = u.TipoUnidade, Quantidade = u.Quantidade }).ToList(),
                Modulos = itemRequest.Modulos.Select(m => new ContratoItemModulo { Id = Guid.NewGuid(), ContratoItemId = itemId, ModuloId = m.ModuloId, ModuloVarianteId = m.ModuloVarianteId, Ativo = m.Ativo, ValorOverride = m.ValorOverride }).ToList()
            });
        }

        var contrato = new Contrato { Id = contratoId, ClienteId = request.ClienteId, RevendaId = request.RevendaId, Ativo = true, DataInicio = request.DataInicio, Itens = itens };

        await contratoRepository.AddAsync(contrato);
        await contratoRepository.SaveChangesAsync();

        return await MontarAdminResponseAsync(contrato);
    }

    public async Task<ContratoAdminResponse?> ObterAdminAsync(Guid id)
    {
        var contrato = await contratoRepository.GetAsync(id);
        return contrato is null ? null : await MontarAdminResponseAsync(contrato);
    }

    public async Task<ContratoPublicoResponse?> ObterPublicoAsync(Guid id, ScopeResult scope)
    {
        var contrato = await contratoRepository.GetAsync(id);
        if (contrato is null || !PertenceAoEscopo(contrato, scope))
        {
            return null;
        }

        return new ContratoPublicoResponse(contrato.Id, contrato.ClienteId, contrato.RevendaId, contrato.Ativo, contrato.DataInicio, contrato.DataFim,
            contrato.Itens.Select(i => new ContratoItemPublicoResponse(i.Id, i.ProdutoId,
                i.Unidades.Select(u => new UnidadeRequest(u.TipoUnidade, u.Quantidade)).ToList(),
                i.Modulos.Where(m => m.Ativo).Select(m => m.ModuloId).ToList())).ToList());
    }

    private static bool PertenceAoEscopo(Contrato contrato, ScopeResult scope) => scope switch
    {
        ScopeResult.SemRestricao => true,
        ScopeResult.RestritoARevenda r => contrato.RevendaId == r.RevendaId,
        _ => false
    };

    private async Task<ContratoAdminResponse> MontarAdminResponseAsync(Contrato contrato)
    {
        var itensResponse = new List<ContratoItemAdminResponse>();

        foreach (var item in contrato.Itens)
        {
            var produto = await produtoRepository.GetAsync(item.ProdutoId) ?? throw new InvalidOperationException("Produto do item nao encontrado.");

            var precosPorUnidade = produto.PrecosPorUnidade.ToDictionary(p => p.TipoUnidade, p => (p.ValorAdesao, p.ValorMensalidade));
            var quantidades = item.Unidades.ToDictionary(u => u.TipoUnidade, u => u.Quantidade);

            var modulosInput = item.Modulos.Select(m =>
            {
                var modulo = produto.Modulos.Single(x => x.Id == m.ModuloId);
                var variante = m.ModuloVarianteId is { } varianteId ? modulo.Variantes.Single(v => v.Id == varianteId) : null;
                return new ModuloPricingInput(m.Ativo, modulo.ValorAdesaoBase, modulo.ValorMensalidadeBase,
                    variante?.TipoUnidadeAplicavel, variante?.ValorAdicionalPorUnidade, m.ValorOverride);
            }).ToList();

            var resultado = PricingResolver.Resolver(new ItemPricingInput(precosPorUnidade, quantidades, modulosInput,
                item.ValorAdesaoOverride, item.ValorMensalidadeOverride, item.TipoDesconto, item.ValorDesconto));

            itensResponse.Add(new ContratoItemAdminResponse(item.Id, item.ProdutoId,
                item.Unidades.Select(u => new UnidadeRequest(u.TipoUnidade, u.Quantidade)).ToList(),
                resultado.ValorAdesaoTotal, resultado.ValorMensalidadeTotal,
                item.ValorAdesaoOverride, item.ValorMensalidadeOverride, item.TipoDesconto, item.ValorDesconto));
        }

        return new ContratoAdminResponse(contrato.Id, contrato.ClienteId, contrato.RevendaId, contrato.Ativo, contrato.DataInicio, contrato.DataFim, itensResponse);
    }
}
```

Register: `services.AddScoped<Application.Contratos.ContratoService>();`

- [ ] **Step 5: Implement the controller**

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VaultApi.Api.Auth;
using VaultApi.Application.Abstractions;
using VaultApi.Application.Contratos;

namespace VaultApi.Api.Controllers;

[ApiController]
[Route("contratos")]
[Authorize]
public class ContratosController(ContratoService service, IScopeFilter scopeFilter) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = PolicyNames.RequireAdmin)]
    public async Task<ActionResult<ContratoAdminResponse>> Criar(CriarContratoRequest request)
    {
        var contrato = await service.CriarAsync(request);
        return CreatedAtAction(nameof(Obter), new { id = contrato.Id }, contrato);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = PolicyNames.RequireRevendaOrAdmin)]
    public async Task<IActionResult> Obter(Guid id)
    {
        var scope = scopeFilter.Resolve();

        if (scope is ScopeResult.SemRestricao)
        {
            var admin = await service.ObterAdminAsync(id);
            return admin is null ? NotFound() : Ok(admin);
        }

        var publico = await service.ObterPublicoAsync(id, scope);
        return publico is null ? NotFound() : Ok(publico);
    }
}
```

- [ ] **Step 6: Run test to verify it passes**

Run: `dotnet test tests/VaultApi.Tests --filter ContratoEndpointTests`
Expected: PASS.

- [ ] **Step 7: Run full suite and commit**

Run: `dotnet test`
Expected: all tests PASS.

```bash
git add -A
git commit -m "feat: add Contrato endpoints with price-hiding DTO split for Revenda/Usuario"
```

---

## Task 14: Licenca (versioned license issuance stub)

**Files:**
- Create: `src/VaultApi.Domain/Entities/Licenca.cs`
- Create: `src/VaultApi.Domain/Repositories/ILicencaRepository.cs`
- Create: `src/VaultApi.Infrastructure/Persistence/Configurations/LicencaConfiguration.cs`
- Create: `src/VaultApi.Infrastructure/Repositories/LicencaRepository.cs`
- Modify: `src/VaultApi.Infrastructure/DependencyInjection.cs`
- Create: `src/VaultApi.Application/Licencas/LicencaService.cs`
- Modify: `src/VaultApi.Application/Contratos/ContratoService.cs` (call license (re)generation after `CriarAsync`)
- Create migration: `src/VaultApi.Infrastructure/Migrations/*_AddLicenca.cs`
- Test: `tests/VaultApi.Tests/Licencas/LicencaServiceTests.cs`

**Interfaces:**
- Consumes: `ContratoItem` (Task 11).
- Produces: `Licenca(Id, ContratoItemId, Serial, Algoritmo, DataEmissao, Status)`, `ILicencaRepository { AddAsync, ListPorItemAsync, SaveChangesAsync }`, `LicencaService.EmitirNovaVersaoAsync(ContratoItem item) : Licenca` — revokes any previous `Ativa` license for the same `ContratoItem` and issues a new one. The actual encryption algorithm is out of scope (per spec); `Serial` is populated with an opaque placeholder string built from the item's content, tagged with `Algoritmo = "PLACEHOLDER-v1"` so it is unmistakably not real crypto and easy to replace later without a schema change.

- [ ] **Step 1: Write the failing test**

```csharp
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using VaultApi.Domain.Entities;
using VaultApi.Domain.Enums;
using VaultApi.Infrastructure.Persistence;
using VaultApi.Infrastructure.Repositories;
using VaultApi.Tests.Fixtures;

namespace VaultApi.Tests.Licencas;

[Collection("Postgres")]
public class LicencaServiceTests(PostgresFixture postgres)
{
    [Fact]
    public async Task Emitting_a_new_version_revokes_the_previous_active_license()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(postgres.ConnectionString).UseSnakeCaseNamingConvention().Options;
        await using (var db = new AppDbContext(options))
        {
            await db.Database.EnsureDeletedAsync();
            await db.Database.MigrateAsync();
        }

        var contratoId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var item = new ContratoItem
        {
            Id = itemId, ContratoId = contratoId, ProdutoId = Guid.NewGuid(),
            Unidades = [ new ContratoItemUnidade { Id = Guid.NewGuid(), ContratoItemId = itemId, TipoUnidade = TipoUnidade.PDV, Quantidade = 5 } ],
            Modulos = []
        };

        await using (var db = new AppDbContext(options))
        {
            db.Set<Cliente>().Add(new Cliente { Id = Guid.NewGuid(), Nome = "C", Cnpj = "1", RevendaId = null });
            await db.SaveChangesAsync();
        }

        Guid primeiraLicencaId;
        await using (var db = new AppDbContext(options))
        {
            var service = new VaultApi.Application.Licencas.LicencaService(new LicencaRepository(db));
            var primeira = await service.EmitirNovaVersaoAsync(item);
            await db.SaveChangesAsync();
            primeiraLicencaId = primeira.Id;
        }

        await using (var db = new AppDbContext(options))
        {
            var service = new VaultApi.Application.Licencas.LicencaService(new LicencaRepository(db));
            item.Unidades[0].Quantidade = 8;
            await service.EmitirNovaVersaoAsync(item);
            await db.SaveChangesAsync();
        }

        await using var readDb = new AppDbContext(options);
        var licencas = await readDb.Set<Licenca>().Where(l => l.ContratoItemId == itemId).ToListAsync();

        licencas.Should().HaveCount(2);
        licencas.Single(l => l.Id == primeiraLicencaId).Status.Should().Be(StatusLicenca.Revogada);
        licencas.Single(l => l.Id != primeiraLicencaId).Status.Should().Be(StatusLicenca.Ativa);
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test tests/VaultApi.Tests --filter LicencaServiceTests`
Expected: FAIL to compile — `Licenca`, `LicencaRepository`, `LicencaService` don't exist.

- [ ] **Step 3: Implement the entity, configuration, repository**

```csharp
using VaultApi.Domain.Enums;

namespace VaultApi.Domain.Entities;

public class Licenca
{
    public Guid Id { get; set; }
    public Guid ContratoItemId { get; set; }
    public string Serial { get; set; } = string.Empty;
    public string Algoritmo { get; set; } = string.Empty;
    public DateTimeOffset DataEmissao { get; set; }
    public StatusLicenca Status { get; set; }
}
```

```csharp
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
```

```csharp
using VaultApi.Domain.Entities;

namespace VaultApi.Domain.Repositories;

public interface ILicencaRepository
{
    Task AddAsync(Licenca licenca);
    Task<List<Licenca>> ListarPorItemAsync(Guid contratoItemId);
}
```

```csharp
using Microsoft.EntityFrameworkCore;
using VaultApi.Domain.Entities;
using VaultApi.Domain.Repositories;
using VaultApi.Infrastructure.Persistence;

namespace VaultApi.Infrastructure.Repositories;

public class LicencaRepository(AppDbContext db) : ILicencaRepository
{
    public async Task AddAsync(Licenca licenca) => await db.Set<Licenca>().AddAsync(licenca);
    public Task<List<Licenca>> ListarPorItemAsync(Guid contratoItemId) => db.Set<Licenca>()
        .Where(l => l.ContratoItemId == contratoItemId)
        .ToListAsync();
}
```

Register: `services.AddScoped<Domain.Repositories.ILicencaRepository, Repositories.LicencaRepository>();`

- [ ] **Step 4: Implement `LicencaService`**

```csharp
using System.Text;
using VaultApi.Domain.Entities;
using VaultApi.Domain.Enums;
using VaultApi.Domain.Repositories;

namespace VaultApi.Application.Licencas;

public class LicencaService(ILicencaRepository repository)
{
    private const string AlgoritmoPlaceholder = "PLACEHOLDER-v1";

    public async Task<Licenca> EmitirNovaVersaoAsync(ContratoItem item)
    {
        var ativas = (await repository.ListarPorItemAsync(item.Id)).Where(l => l.Status == StatusLicenca.Ativa);
        foreach (var ativa in ativas)
        {
            ativa.Status = StatusLicenca.Revogada;
        }

        var licenca = new Licenca
        {
            Id = Guid.NewGuid(),
            ContratoItemId = item.Id,
            Serial = ConstruirSerialOpaco(item),
            Algoritmo = AlgoritmoPlaceholder,
            DataEmissao = DateTimeOffset.UtcNow,
            Status = StatusLicenca.Ativa
        };

        await repository.AddAsync(licenca);
        return licenca;
    }

    private static string ConstruirSerialOpaco(ContratoItem item)
    {
        var conteudo = new StringBuilder()
            .Append(item.ProdutoId).Append('|')
            .Append(string.Join(',', item.Unidades.Select(u => $"{u.TipoUnidade}:{u.Quantidade}")))
            .Append('|')
            .Append(string.Join(',', item.Modulos.Where(m => m.Ativo).Select(m => m.ModuloId)));

        return Convert.ToBase64String(Encoding.UTF8.GetBytes(conteudo.ToString()));
    }
}
```

Register: `services.AddScoped<Application.Licencas.LicencaService>();`

- [ ] **Step 5: Wire license issuance into contract creation**

In `src/VaultApi.Application/Contratos/ContratoService.cs`, add `Licencas.LicencaService licencaService` to the primary constructor and call it in `CriarAsync` before the final `SaveChangesAsync`:

```csharp
// constructor becomes:
// public class ContratoService(IContratoRepository contratoRepository, IProdutoRepository produtoRepository, Licencas.LicencaService licencaService)

// inside CriarAsync, after building `itens` and before `await contratoRepository.AddAsync(contrato);`:
foreach (var item in itens)
{
    await licencaService.EmitirNovaVersaoAsync(item);
}
```

- [ ] **Step 6: Generate the migration**

```bash
dotnet ef migrations add AddLicenca \
  --project src/VaultApi.Infrastructure \
  --startup-project src/VaultApi.Api \
  --output-dir Migrations
```

- [ ] **Step 7: Run test to verify it passes**

Run: `dotnet test tests/VaultApi.Tests --filter LicencaServiceTests`
Expected: PASS.

- [ ] **Step 8: Run full suite and commit**

Run: `dotnet test`
Expected: all tests PASS.

```bash
git add -A
git commit -m "feat: add versioned Licenca issuance stub, wired into contract creation"
```

---

## Task 15: README and Final Verification

**Files:**
- Create: `README.md`

**Interfaces:**
- Consumes: nothing (documentation only).
- Produces: nothing consumed by other tasks — this is the terminal task.

- [ ] **Step 1: Write the README**

```markdown
# VaultApi

API .NET 10 para gestao de usuarios, clientes, revendas, produtos e contratos —
a relacao comercial cliente/revenda que licencia o uso dos produtos.

## Dominio

- Usuario tem 1 Nivel: `Admin`, `Revenda` ou `Usuario`. Usuario/Cliente sem
  `RevendaId` pertence a matriz.
- Produto tem preco por tipo de unidade (`Servidor`, `Estacao`, `PDA`, `PDV`)
  via `ProdutoPrecoUnidade`, e Modulos (com Variantes que cobram adicional por
  unidade, ex.: TEF por PDV).
- Contrato e a fonte da verdade do licenciamento: define quantidade por tipo
  de unidade e modulos ativos por produto, com overrides de preco (fixo,
  percentual ou valor) por item.
- So Admin escreve Contrato/Revenda/Usuario/Produto. Revenda e Usuario tem
  leitura restrita a propria `RevendaId` e nunca veem valores negociados.
- Cada item de contrato gera uma Licenca (serial opaco versionado); o
  algoritmo de criptografia real ainda sera definido.

Detalhes completos: `docs/superpowers/specs/2026-08-29-vault-api-design.md`.

## Stack

.NET 10, ASP.NET Core Identity, EF Core + Npgsql, PostgreSQL, JWT Bearer,
xUnit + FluentAssertions + Testcontainers.

## Rodando localmente

```bash
docker compose up -d
dotnet ef database update --project src/VaultApi.Infrastructure --startup-project src/VaultApi.Api
dotnet run --project src/VaultApi.Api
```

## Rodando os testes

Requer Docker (Testcontainers sobe um Postgres real por execucao):

```bash
dotnet test
```

## Estrutura

```
src/VaultApi.Domain          entidades, enums, interfaces de repositorio
src/VaultApi.Application     services, DTOs, PricingResolver, IScopeFilter
src/VaultApi.Infrastructure  EF Core, Identity, repositorios, migrations
src/VaultApi.Api             controllers, JWT, policies de autorizacao
tests/VaultApi.Tests         testes unitarios e de integracao
```

## Convencoes

- `nivel` e `revenda_id` sao claims do JWT; `RequireAdmin` e
  `RequireRevendaOrAdmin` sao as policies de autorizacao.
- Escopo de dado (Revenda/Usuario so veem a propria `RevendaId`) e aplicado
  no repositorio via `IScopeFilter`, nao no controller — nunca filtre so na
  camada de apresentacao.
- Migrations aplicam automaticamente so em Development; producao aplica via
  `dotnet ef database update` manual/CI.
```

- [ ] **Step 2: Run the full test suite**

Run: `dotnet test`
Expected: all tests across all 15 tasks PASS.

- [ ] **Step 3: Verify the full solution builds clean**

Run: `dotnet build VaultApi.slnx`
Expected: `Build succeeded.` with 0 errors, 0 warnings.

- [ ] **Step 4: Commit**

```bash
git add -A
git commit -m "docs: add project README"
```
