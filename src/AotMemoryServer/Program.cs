using AotMemoryServer.Application.Serialization;
using AotMemoryServer.Data;
using AotMemoryServer.Endpoints;
using AotMemoryServer.Models;
using AotMemoryServer.Application.Abstractions;
using AotMemoryServer.Application.Commands;
using AotMemoryServer.Application.Queries;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonContext.Default));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultDb") ?? "Data Source=memory.db"));

builder.Services.AddScoped<IQueryHandler<GetFacts, PagedResult<MemoryFact>>, GetFactsHandler>();
builder.Services.AddScoped<IQueryHandler<GetFactById, MemoryFact?>, GetFactByIdHandler>();
builder.Services.AddScoped<IQueryHandler<SearchFacts, PagedResult<MemoryFact>>, SearchFactsHandler>();
builder.Services.AddScoped<ICommandHandler<UpsertFact, MemoryFact>, UpsertFactHandler>();
builder.Services.AddScoped<ICommandHandler<UpdateFact, MemoryFact?>, UpdateFactHandler>();
builder.Services.AddScoped<ICommandHandler<DeleteFact, bool>, DeleteFactHandler>();

var app = builder.Build();

if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ASPNETCORE_URLS")))
    app.Urls.Add("http://0.0.0.0:5070");

using var scope = app.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
await db.Database.MigrateAsync();

app.MapMemoryEndpoints();
app.MapHealthEndpoints();
app.MapMcpEndpoints();

app.Run();

public partial class Program { }
