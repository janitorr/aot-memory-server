using Mittens.Core.Fact;
using Mittens.Host.Memory.Data;
using Mittens.Host.Memory.Data.Compiled;
using Mittens.Host.Memory.Endpoints;
using Mittens.Host.Endpoints;
using Mittens.Host.Serialization;
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

builder.Services.AddScoped<IFactReader, FactReader>();
builder.Services.AddScoped<IFactWriter, FactWriter>();

builder.Services.AddMediator(options =>
{
    options.ServiceLifetime = ServiceLifetime.Scoped;
});

builder.Services.AddMcpServer()
    .WithHttpTransport(opts => opts.Stateless = true)
    .WithTools<FactMcpTools>();

builder.Services.AddOpenApi();

var app = builder.Build();

if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ASPNETCORE_URLS")))
    app.Urls.Add("http://0.0.0.0:5070");

using var scope = app.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
await db.Database.CanConnectAsync();
await db.Database.ExecuteSqlRawAsync("""
    CREATE TABLE IF NOT EXISTS "MittensFacts" (
        "Id" INTEGER NOT NULL CONSTRAINT "PK_MittensFacts" PRIMARY KEY AUTOINCREMENT,
        "Category" TEXT NOT NULL,
        "Key" TEXT NOT NULL,
        "Value" TEXT NOT NULL,
        "Scope" TEXT NOT NULL,
        "Confidence" REAL NOT NULL,
        "Source" TEXT NULL,
        "UpdatedAt" TEXT NOT NULL,
        "IsDeprecated" INTEGER NOT NULL
    );
    CREATE UNIQUE INDEX IF NOT EXISTS "IX_MittensFacts_Category_Key_Scope" ON "MittensFacts" ("Category", "Key", "Scope");
    CREATE INDEX IF NOT EXISTS "IX_MittensFacts_Scope" ON "MittensFacts" ("Scope");
    """);

app.MapOpenApi();
app.MapScalarApiReference();

app.MapFactEndpoints();
app.MapHealthEndpoints();
app.MapMcp("/mcp");

app.Run();

public partial class Program { }
