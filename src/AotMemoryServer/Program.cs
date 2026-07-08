using AotMemoryServer.Application.Serialization;
using AotMemoryServer.Data;
using AotMemoryServer.Data.Compiled;
using AotMemoryServer.Endpoints;
using Mediator;
using Microsoft.EntityFrameworkCore;
using ModelContextProtocol.AspNetCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonContext.Default));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseModel(AppDbContextModel.Instance)
           .UseSqlite(builder.Configuration.GetConnectionString("DefaultDb") ?? "Data Source=memory.db"));

builder.Services.AddMediator(options =>
{
    options.ServiceLifetime = ServiceLifetime.Scoped;
});

builder.Services.AddMcpServer()
    .WithHttpTransport(opts => opts.Stateless = true)
    .WithTools<MemoryMcpTools>();

builder.Services.AddOpenApi();

var app = builder.Build();

if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ASPNETCORE_URLS")))
    app.Urls.Add("http://0.0.0.0:5070");

using var scope = app.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
await db.Database.CanConnectAsync();
await db.Database.ExecuteSqlRawAsync("""
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
    """);

app.MapOpenApi();
app.MapScalarApiReference();

app.MapMemoryEndpoints();
app.MapHealthEndpoints();
app.MapMcp("/mcp");

app.Run();

public partial class Program { }
