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
