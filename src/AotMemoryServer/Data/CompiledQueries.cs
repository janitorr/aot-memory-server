using Microsoft.EntityFrameworkCore;
using AotMemoryServer.Models;

namespace AotMemoryServer.Data;

public static partial class CompiledQueries
{
    public static readonly Func<AppDbContext, int, Task<MemoryFact?>> GetByIdAsync =
        EF.CompileAsyncQuery(
            (AppDbContext db, int id) =>
                db.MemoryFacts
                    .FromSqlRaw("""
                        SELECT "Id", "Category", "Key", "Value", "Scope", "Confidence", "Source", "UpdatedAt", "IsDeprecated"
                        FROM "MemoryFacts" WHERE "Id" = @p0
                        """, id)
                    .SingleOrDefault()
        );

    public static readonly Func<AppDbContext, string, string, string, Task<MemoryFact?>> GetByCategoryKeyScopeAsync =
        EF.CompileAsyncQuery(
            (AppDbContext db, string category, string key, string scope) =>
                db.MemoryFacts
                    .FromSqlRaw("""
                        SELECT "Id", "Category", "Key", "Value", "Scope", "Confidence", "Source", "UpdatedAt", "IsDeprecated"
                        FROM "MemoryFacts" WHERE "Category" = @p0 AND "Key" = @p1 AND "Scope" = @p2
                        """, category, key, scope)
                    .SingleOrDefault()
        );

    public static readonly Func<AppDbContext, string?, string?, string?, Task<int>> GetFactsCountAsync =
        EF.CompileAsyncQuery(
            (AppDbContext db, string? category, string? scope, string? key) =>
                db.Database
                    .SqlQueryRaw<int>("""
                        SELECT COUNT(*) AS "Value" FROM "MemoryFacts"
                        WHERE (@p0 IS NULL OR "Category" = @p0) AND (@p1 IS NULL OR "Scope" = @p1) AND (@p2 IS NULL OR "Key" = @p2)
                        """, category, scope, key)
                    .Single()
        );

    public static readonly Func<AppDbContext, string?, string?, string?, int, int, Task<List<MemoryFact>>> GetFactsPageAsync =
        EF.CompileAsyncQuery(
            (AppDbContext db, string? category, string? scope, string? key, int limit, int offset) =>
                db.MemoryFacts
                    .FromSqlRaw("""
                        SELECT "Id", "Category", "Key", "Value", "Scope", "Confidence", "Source", "UpdatedAt", "IsDeprecated"
                        FROM "MemoryFacts"
                        WHERE (@p0 IS NULL OR "Category" = @p0) AND (@p1 IS NULL OR "Scope" = @p1) AND (@p2 IS NULL OR "Key" = @p2)
                        ORDER BY "Category", "Key" LIMIT @p3 OFFSET @p4
                        """, category, scope, key, limit, offset)
                    .ToList()
        );

    public static readonly Func<AppDbContext, string, string?, string?, Task<int>> SearchFactsCountAsync =
        EF.CompileAsyncQuery(
            (AppDbContext db, string q, string? category, string? scope) =>
                db.Database
                    .SqlQueryRaw<int>("""
                        SELECT COUNT(*) AS "Value" FROM "MemoryFacts"
                        WHERE (@p0 IS NULL OR "Key" LIKE '%' || @p0 || '%' OR "Value" LIKE '%' || @p0 || '%') AND (@p1 IS NULL OR "Category" = @p1) AND (@p2 IS NULL OR "Scope" = @p2)
                        """, q, category, scope)
                    .Single()
        );

    public static readonly Func<AppDbContext, string, string?, string?, int, int, Task<List<MemoryFact>>> SearchFactsPageAsync =
        EF.CompileAsyncQuery(
            (AppDbContext db, string q, string? category, string? scope, int limit, int offset) =>
                db.MemoryFacts
                    .FromSqlRaw("""
                        SELECT "Id", "Category", "Key", "Value", "Scope", "Confidence", "Source", "UpdatedAt", "IsDeprecated"
                        FROM "MemoryFacts"
                        WHERE (@p0 IS NULL OR "Key" LIKE '%' || @p0 || '%' OR "Value" LIKE '%' || @p0 || '%') AND (@p1 IS NULL OR "Category" = @p1) AND (@p2 IS NULL OR "Scope" = @p2)
                        ORDER BY "Category", "Key" LIMIT @p3 OFFSET @p4
                        """, q, category, scope, limit, offset)
                    .ToList()
        );
}
