using AotMemoryServer.Data;
using AotMemoryServer.Models;
using AotMemoryServer.Application.Abstractions;
using AotMemoryServer.Application.Commands;
using AotMemoryServer.Application.Queries;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultDb") ?? "Data Source=memory.db"));

builder.Services.AddScoped<IQueryHandler<GetFacts, PagedResult<MemoryFact>>, GetFactsHandler>();
builder.Services.AddScoped<IQueryHandler<GetFactById, MemoryFact?>, GetFactByIdHandler>();
builder.Services.AddScoped<IQueryHandler<SearchFacts, PagedResult<MemoryFact>>, SearchFactsHandler>();
builder.Services.AddScoped<ICommandHandler<UpsertFact, MemoryFact>, UpsertFactHandler>();
builder.Services.AddScoped<ICommandHandler<UpdateFact, MemoryFact?>, UpdateFactHandler>();
builder.Services.AddScoped<ICommandHandler<DeleteFact, bool>, DeleteFactHandler>();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
