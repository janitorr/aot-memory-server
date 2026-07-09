namespace AotMemoryServer.Data;

public static class MemoryFactSql
{
    public const string GetById = """
        SELECT "Id", "Category", "Key", "Value", "Scope", "Confidence", "Source", "UpdatedAt", "IsDeprecated"
        FROM "MemoryFacts" WHERE "Id" = @p0
        """;

    public const string GetByCategoryKeyScope = """
        SELECT "Id", "Category", "Key", "Value", "Scope", "Confidence", "Source", "UpdatedAt", "IsDeprecated"
        FROM "MemoryFacts" WHERE "Category" = @p0 AND "Key" = @p1 AND "Scope" = @p2
        """;

    public const string GetFactsCount = """
        SELECT COUNT(*) AS "Value" FROM "MemoryFacts"
        WHERE (@p0 IS NULL OR "Category" = @p0) AND (@p1 IS NULL OR "Scope" = @p1) AND (@p2 IS NULL OR "Key" = @p2)
        """;

    public const string GetFactsPage = """
        SELECT "Id", "Category", "Key", "Value", "Scope", "Confidence", "Source", "UpdatedAt", "IsDeprecated"
        FROM "MemoryFacts"
        WHERE (@p0 IS NULL OR "Category" = @p0) AND (@p1 IS NULL OR "Scope" = @p1) AND (@p2 IS NULL OR "Key" = @p2)
        ORDER BY "Category", "Key" LIMIT @p3 OFFSET @p4
        """;

    public const string SearchFactsCount = """
        SELECT COUNT(*) AS "Value" FROM "MemoryFacts"
        WHERE (@p0 IS NULL OR "Key" LIKE '%' || @p0 || '%' OR "Value" LIKE '%' || @p0 || '%') AND (@p1 IS NULL OR "Category" = @p1) AND (@p2 IS NULL OR "Scope" = @p2)
        """;

    public const string SearchFactsPage = """
        SELECT "Id", "Category", "Key", "Value", "Scope", "Confidence", "Source", "UpdatedAt", "IsDeprecated"
        FROM "MemoryFacts"
        WHERE (@p0 IS NULL OR "Key" LIKE '%' || @p0 || '%' OR "Value" LIKE '%' || @p0 || '%') AND (@p1 IS NULL OR "Category" = @p1) AND (@p2 IS NULL OR "Scope" = @p2)
        ORDER BY "Category", "Key" LIMIT @p3 OFFSET @p4
        """;
}
