using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Mediator;
using AotMemoryServer.Data;
using AotMemoryServer.Models;
using AotMemoryServer.Application.Abstractions;

namespace AotMemoryServer.Application.Queries;

public sealed record GetFacts(string? Category, string? Scope, string? Key, int Page = 1, int PageSize = 20)
    : IRequest<PagedResult<MemoryFact>>;

public sealed class GetFactsHandler(AppDbContext db) : IRequestHandler<GetFacts, PagedResult<MemoryFact>>
{
    public async ValueTask<PagedResult<MemoryFact>> Handle(GetFacts query, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Max(1, Math.Min(100, query.PageSize));

        var conditions = new List<string>();
        var parameters = new List<SqliteParameter>();

        if (query.Category is not null)
        {
            conditions.Add("\"Category\" = @p" + parameters.Count);
            parameters.Add(new SqliteParameter("@p" + parameters.Count, query.Category));
        }
        if (query.Scope is not null)
        {
            conditions.Add("\"Scope\" = @p" + parameters.Count);
            parameters.Add(new SqliteParameter("@p" + parameters.Count, query.Scope));
        }
        if (query.Key is not null)
        {
            conditions.Add("\"Key\" = @p" + parameters.Count);
            parameters.Add(new SqliteParameter("@p" + parameters.Count, query.Key));
        }

        var whereClause = conditions.Count > 0
            ? "WHERE " + string.Join(" AND ", conditions)
            : "";

        var totalCountParams = parameters.ToArray();

        var totalCount = 0;
        var items = new List<MemoryFact>();

        var conn = db.Database.GetDbConnection();
        await conn.OpenAsync(cancellationToken);

        try
        {
            await using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*) FROM \"MemoryFacts\" " + whereClause;
                foreach (var p in totalCountParams)
                    cmd.Parameters.Add(p);
                var result = await cmd.ExecuteScalarAsync(cancellationToken);
                totalCount = result is long l ? (int)l : 0;
            }

            await using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT \"Id\", \"Category\", \"Key\", \"Value\", \"Scope\", \"Confidence\", \"Source\", \"UpdatedAt\", \"IsDeprecated\" FROM \"MemoryFacts\" " + whereClause + " ORDER BY \"Category\", \"Key\" LIMIT @p" + parameters.Count + " OFFSET @p" + (parameters.Count + 1);
                foreach (var p in parameters)
                    cmd.Parameters.Add(p);
                cmd.Parameters.Add(new SqliteParameter("@p" + parameters.Count, pageSize));
                cmd.Parameters.Add(new SqliteParameter("@p" + (parameters.Count + 1), (page - 1) * pageSize));

                await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
                while (await reader.ReadAsync(cancellationToken))
                {
                    items.Add(new MemoryFact
                    {
                        Id = reader.GetInt32(0),
                        Category = reader.GetString(1),
                        Key = reader.GetString(2),
                        Value = reader.GetString(3),
                        Scope = reader.GetString(4),
                        Confidence = reader.GetDouble(5),
                        Source = reader.IsDBNull(6) ? null : reader.GetString(6),
                        UpdatedAt = DateTimeOffset.Parse(reader.GetString(7)),
                        IsDeprecated = reader.GetBoolean(8)
                    });
                }
            }
        }
        finally
        {
            await conn.CloseAsync();
        }

        return new PagedResult<MemoryFact>(items, totalCount, page, pageSize);
    }
}
