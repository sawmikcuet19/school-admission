using Microsoft.EntityFrameworkCore;
using SchoolAdmission.Api.Data;
using Testcontainers.PostgreSql;

namespace SchoolAdmission.Tests;

public class PostgresFixture : IAsyncLifetime
{
    private PostgreSqlContainer container = null!;

    public string ConnectionString { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        container = new PostgreSqlBuilder("postgres:18")
            .WithDatabase("school_admission_test")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        await container.StartAsync();
        ConnectionString = container.GetConnectionString();

        await using var db = CreateDbContext();
        await db.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await container.DisposeAsync();
    }

    public AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        return new AppDbContext(options);
    }
}

[CollectionDefinition(nameof(PostgresCollection))]
public class PostgresCollection : ICollectionFixture<PostgresFixture>
{
}
