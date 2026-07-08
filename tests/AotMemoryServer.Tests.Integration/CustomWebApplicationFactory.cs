using AotMemoryServer.Data;
using AotMemoryServer.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace AotMemoryServer.Tests.Integration;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private SqliteConnection? _connection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor is not null)
                services.Remove(descriptor);

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite("Data Source=file::memory:?cache=shared"));
        });
    }

    public async Task InitializeAsync()
    {
        _connection = new SqliteConnection("Data Source=file::memory:?cache=shared");
        await _connection.OpenAsync();

        await using var cmd = _connection.CreateCommand();
        cmd.CommandText = """
            CREATE TABLE IF NOT EXISTS "MemoryFacts" (
                "Id" INTEGER NOT NULL CONSTRAINT "PK_MemoryFacts" PRIMARY KEY AUTOINCREMENT,
                "Category" TEXT NOT NULL,
                "Key" TEXT NOT NULL,
                "Value" TEXT NOT NULL,
                "Scope" TEXT NOT NULL,
                "Confidence" REAL NOT NULL,
                "Source" TEXT NULL,
                "UpdatedAt" TEXT NOT NULL,
                "IsDeprecated" INTEGER NOT NULL
            );
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_MemoryFacts_Category_Key_Scope" ON "MemoryFacts" ("Category", "Key", "Scope");
            CREATE INDEX IF NOT EXISTS "IX_MemoryFacts_Scope" ON "MemoryFacts" ("Scope");
            """;
        await cmd.ExecuteNonQueryAsync();
    }

    public new async Task DisposeAsync()
    {
        if (_connection is not null)
        {
            await _connection.DisposeAsync();
        }
    }

    public async Task<MemoryFact> CreateFactAsync(MemoryFact fact)
    {
        fact.UpdatedAt = DateTimeOffset.UtcNow;

        if (_connection!.State != System.Data.ConnectionState.Open)
            await _connection.OpenAsync();

        await using var cmd = _connection.CreateCommand();
        cmd.CommandText = "INSERT INTO \"MemoryFacts\" (\"Category\", \"Key\", \"Value\", \"Scope\", \"Confidence\", \"Source\", \"UpdatedAt\", \"IsDeprecated\") VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7); SELECT last_insert_rowid();";
        cmd.Parameters.Add(new SqliteParameter("@p0", fact.Category));
        cmd.Parameters.Add(new SqliteParameter("@p1", fact.Key));
        cmd.Parameters.Add(new SqliteParameter("@p2", fact.Value));
        cmd.Parameters.Add(new SqliteParameter("@p3", fact.Scope));
        cmd.Parameters.Add(new SqliteParameter("@p4", fact.Confidence));
        cmd.Parameters.Add(new SqliteParameter("@p5", fact.Source ?? (object)DBNull.Value));
        cmd.Parameters.Add(new SqliteParameter("@p6", fact.UpdatedAt.ToString("O")));
        cmd.Parameters.Add(new SqliteParameter("@p7", fact.IsDeprecated));
        var id = await cmd.ExecuteScalarAsync();
        fact.Id = id is long l ? (int)l : 0;

        return fact;
    }

    public async Task ClearFactsAsync()
    {
        if (_connection!.State != System.Data.ConnectionState.Open)
            await _connection.OpenAsync();

        await using var cmd = _connection.CreateCommand();
        cmd.CommandText = "DELETE FROM \"MemoryFacts\"";
        await cmd.ExecuteNonQueryAsync();
    }
}
