namespace AotMemoryServer.Application.Abstractions;

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize);
