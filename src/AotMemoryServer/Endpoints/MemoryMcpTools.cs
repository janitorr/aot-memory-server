using System.ComponentModel;
using System.Text.Json;
using AotMemoryServer.Application.Abstractions;
using AotMemoryServer.Application.Commands;
using AotMemoryServer.Application.Queries;
using AotMemoryServer.Application.Serialization;
using AotMemoryServer.Models;
using ModelContextProtocol.Server;

namespace AotMemoryServer.Endpoints;

[McpServerToolType]
public sealed class MemoryMcpTools
{
    private readonly IQueryHandler<GetFacts, PagedResult<MemoryFact>> _listHandler;
    private readonly IQueryHandler<GetFactById, MemoryFact?> _getByIdHandler;
    private readonly IQueryHandler<SearchFacts, PagedResult<MemoryFact>> _searchHandler;
    private readonly ICommandHandler<UpsertFact, MemoryFact> _upsertHandler;
    private readonly ICommandHandler<UpdateFact, MemoryFact?> _updateHandler;
    private readonly ICommandHandler<DeleteFact, bool> _deleteHandler;

    public MemoryMcpTools(
        IQueryHandler<GetFacts, PagedResult<MemoryFact>> listHandler,
        IQueryHandler<GetFactById, MemoryFact?> getByIdHandler,
        IQueryHandler<SearchFacts, PagedResult<MemoryFact>> searchHandler,
        ICommandHandler<UpsertFact, MemoryFact> upsertHandler,
        ICommandHandler<UpdateFact, MemoryFact?> updateHandler,
        ICommandHandler<DeleteFact, bool> deleteHandler)
    {
        _listHandler = listHandler;
        _getByIdHandler = getByIdHandler;
        _searchHandler = searchHandler;
        _upsertHandler = upsertHandler;
        _updateHandler = updateHandler;
        _deleteHandler = deleteHandler;
    }

    [McpServerTool]
    [Description("List memory facts with optional filtering and pagination")]
    public async Task<string> MemoryList(
        [Description("Filter by category (preference, fact, concept, rule, plan, goal, task, note)")]
        string? category = null,
        [Description("Filter by scope/namespace")]
        string? scope = null,
        [Description("Filter by key")]
        string? key = null,
        [Description("Page number (starts at 1, default 1)")]
        int page = 1,
        [Description("Items per page (default 20, max 100)")]
        int pageSize = 20)
    {
        var result = await _listHandler.Handle(new GetFacts(category, scope, key, page, pageSize));
        return JsonSerializer.Serialize(result, AppJsonContext.Default.PagedResultMemoryFact);
    }

    [McpServerTool]
    [Description("Get a single memory fact by ID")]
    public async Task<string> MemoryGet(
        [Description("Fact ID")] int id)
    {
        var result = await _getByIdHandler.Handle(new GetFactById(id));
        return JsonSerializer.Serialize(result, AppJsonContext.Default.MemoryFact);
    }

    [McpServerTool]
    [Description("Search memory facts by keyword in key and value fields")]
    public async Task<string> MemorySearch(
        [Description("Search keyword")] string q,
        [Description("Filter by category")]
        string? category = null,
        [Description("Filter by scope/namespace")]
        string? scope = null,
        [Description("Page number (starts at 1, default 1)")]
        int page = 1,
        [Description("Items per page (default 20, max 100)")]
        int pageSize = 20)
    {
        var result = await _searchHandler.Handle(new SearchFacts(q, category, scope, page, pageSize));
        return JsonSerializer.Serialize(result, AppJsonContext.Default.PagedResultMemoryFact);
    }

    [McpServerTool]
    [Description("Create or replace a memory fact. If a fact with the same category/key/scope exists, the one with higher confidence wins. Returns the stored fact.")]
    public async Task<string> MemorySet(
        [Description("Category (preference, fact, concept, rule, plan, goal, task, note)")]
        string category,
        [Description("Unique key within the scope")]
        string key,
        [Description("Value content (max 10,000 characters)")]
        string value,
        [Description("Scope/namespace, e.g. project, feature, area name")]
        string scope,
        [Description("Confidence score 0-1 (default 1.0)")]
        double confidence = 1.0,
        [Description("Optional source identifier")]
        string? source = null)
    {
        var fact = new MemoryFact
        {
            Category = category,
            Key = key,
            Value = value,
            Scope = scope,
            Confidence = confidence,
            Source = source
        };

        var result = await _upsertHandler.Handle(new UpsertFact(fact));
        return JsonSerializer.Serialize(result, AppJsonContext.Default.MemoryFact);
    }

    [McpServerTool]
    [Description("Update an existing memory fact by ID. Only provided fields are changed.")]
    public async Task<string> MemoryUpdate(
        [Description("Fact ID to update")] int id,
        [Description("New category")]
        string? category = null,
        [Description("New key")]
        string? key = null,
        [Description("New value content (max 10,000 characters)")]
        string? value = null,
        [Description("New scope/namespace")]
        string? scope = null,
        [Description("New confidence score 0-1")]
        double? confidence = null,
        [Description("New source identifier")]
        string? source = null)
    {
        var existing = await _getByIdHandler.Handle(new GetFactById(id));
        if (existing is null)
            return """{"error":"Fact not found"}""";

        var updated = new MemoryFact
        {
            Id = existing.Id,
            Category = category ?? existing.Category,
            Key = key ?? existing.Key,
            Value = value ?? existing.Value,
            Scope = scope ?? existing.Scope,
            Confidence = confidence ?? existing.Confidence,
            Source = source ?? existing.Source,
            IsDeprecated = existing.IsDeprecated,
            UpdatedAt = existing.UpdatedAt
        };

        var result = await _updateHandler.Handle(new UpdateFact(id, updated));
        return JsonSerializer.Serialize(result, AppJsonContext.Default.MemoryFact);
    }

    [McpServerTool]
    [Description("Delete a memory fact by ID")]
    public async Task<string> MemoryDelete(
        [Description("Fact ID to delete")] int id)
    {
        var result = await _deleteHandler.Handle(new DeleteFact(id));
        return JsonSerializer.Serialize(result);
    }
}
