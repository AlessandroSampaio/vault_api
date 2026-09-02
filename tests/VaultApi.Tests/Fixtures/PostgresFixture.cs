using Testcontainers.PostgreSql;

namespace VaultApi.Tests.Fixtures;

public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly string? _localConnectionString =
        Environment.GetEnvironmentVariable("VAULTAPI_TEST_PG_CONNECTION");

    private readonly PostgreSqlContainer? _container;

    public PostgresFixture()
    {
        if (_localConnectionString is null)
        {
            _container = new PostgreSqlBuilder()
                .WithImage("postgres:16-alpine")
                .WithDatabase("vaultapi")
                .WithUsername("vaultapi")
                .WithPassword("vaultapi")
                .Build();
        }
    }

    public string ConnectionString => _localConnectionString ?? _container!.GetConnectionString();

    public Task InitializeAsync() => _container?.StartAsync() ?? Task.CompletedTask;

    public Task DisposeAsync() => _container?.DisposeAsync().AsTask() ?? Task.CompletedTask;
}

[CollectionDefinition("Postgres")]
public sealed class PostgresCollection : ICollectionFixture<PostgresFixture>;
