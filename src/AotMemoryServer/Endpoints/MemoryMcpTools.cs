using System.ComponentModel;
using System.Text.Json;
using AotMemoryServer.Application.Commands;
using AotMemoryServer.Application.Queries;
using AotMemoryServer.Application.Serialization;
using AotMemoryServer.Models;
using Mediator;
using ModelContextProtocol.Server;

namespace AotMemoryServer.Endpoints;

[McpServerToolType]
public sealed class MemoryMcpTools
{
    private readonly ISender _sender;

    public MemoryMcpTools(ISender sender)
    {
        _sender = sender;
    }

    [McpServerTool]
    [Description("List memory facts with optional filtering and pagination. Use when the user asks 'do you remember?', 'what did I tell you about X?', 'what do you know about X?', or wants to recall what's stored.")]
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
        var result = await _sender.Send(new GetFacts(category, scope, key, page, pageSize));
        return JsonSerializer.Serialize(result, AppJsonContext.Default.PagedResultMemoryFact);
    }

    [McpServerTool]
    [Description("Get a single memory fact by ID. Use when you need the full details of a specific fact you already know the ID of.")]
    public async Task<string> MemoryGet(
        [Description("Fact ID")] int id)
    {
        var result = await _sender.Send(new GetFactById(id));
        return JsonSerializer.Serialize(result, AppJsonContext.Default.MemoryFact);
    }

    [McpServerTool]
    [Description("Search memory facts by keyword in key and value fields. Use when the user asks 'do you remember?', 'what did I tell you about X?', 'what do you know about X?', or wants to recall stored context but you're not sure of the exact key.")]
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
        var result = await _sender.Send(new SearchFacts(q, category, scope, page, pageSize));
        return JsonSerializer.Serialize(result, AppJsonContext.Default.PagedResultMemoryFact);
    }

    [McpServerTool]
    [Description("Create or replace a memory fact. If a fact with the same category/key/scope exists, the one with higher confidence wins. Returns the stored fact. Use when the user says 'remember', 'save that', 'note this', 'keep this for later', 'store this', or instructs you to save information.")]
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

        var result = await _sender.Send(new UpsertFact(fact));
        return JsonSerializer.Serialize(result, AppJsonContext.Default.MemoryFact);
    }

    [McpServerTool]
    [Description("Update an existing memory fact by ID. Only provided fields are changed. Use when the user wants to change, correct, or update something already stored.")]
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
        var existing = await _sender.Send(new GetFactById(id));
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

        var result = await _sender.Send(new UpdateFact(id, updated));
        return JsonSerializer.Serialize(result, AppJsonContext.Default.MemoryFact);
    }

    [McpServerTool]
    [Description("Delete a memory fact by ID. Use when the user says 'forget', 'remove', 'delete that memory', or wants something removed from storage.")]
    public async Task<string> MemoryDelete(
        [Description("Fact ID to delete")] int id)
    {
        var result = await _sender.Send(new DeleteFact(id));
        return result ? "true" : "false";
    }
}
