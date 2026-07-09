using System.ComponentModel;
using System.Text.Json;
using Mittens.Application.Commands;
using Mittens.Application.Queries;
using Mittens.Application.Serialization;
using Mittens.Models;
using Mediator;
using ModelContextProtocol.Server;

namespace Mittens.Endpoints;

[McpServerToolType]
public sealed class MittensMcpTools
{
    private readonly ISender _sender;

    public MittensMcpTools(ISender sender)
    {
        _sender = sender;
    }

    [McpServerTool]
    [Description("Use when the user asks to browse stored information ('what do you remember?', 'list what you've stored'), or when mittens_search returns no results.")]
    public async Task<string> MittensList(
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
        return JsonSerializer.Serialize(result, AppJsonContext.Default.PagedResultMittensFact);
    }

    [McpServerTool]
    [Description("Get a single memory fact by ID. Use when you need the full details of a specific fact you already know the ID of.")]
    public async Task<string> MittensGet(
        [Description("Fact ID")] int id)
    {
        var result = await _sender.Send(new GetFactById(id));
        return JsonSerializer.Serialize(result, AppJsonContext.Default.MittensFact);
    }

    [McpServerTool]
    [Description("Use whenever the user asks about their preferences, project context, past decisions, coding conventions, or past instructions — as well as recall requests like 'do you remember?' or 'what did I tell you about X?'. Always search before answering 'I don't know' to questions about the user's preferences or project setup.")]
    public async Task<string> MittensSearch(
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
        return JsonSerializer.Serialize(result, AppJsonContext.Default.PagedResultMittensFact);
    }

    [McpServerTool]
    [Description("Use when the user asks to save information ('remember', 'save that', 'note this', 'store this'), or shares preferences, conventions, or decisions that should persist across sessions. If a fact with the same category/key/scope exists, the one with higher confidence wins.")]
    public async Task<string> MittensSet(
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
        var fact = new MittensFact
        {
            Category = category,
            Key = key,
            Value = value,
            Scope = scope,
            Confidence = confidence,
            Source = source
        };

        var result = await _sender.Send(new UpsertFact(fact));
        return JsonSerializer.Serialize(result, AppJsonContext.Default.MittensFact);
    }

    [McpServerTool]
    [Description("Use when the user wants to modify a stored fact ('that's wrong', 'actually...', 'update that', 'change X'), or to refine something saved earlier. Requires the fact's ID from a prior search or list. Only provided fields are changed; omit fields to keep current values.")]
    public async Task<string> MittensUpdate(
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

        var updated = new MittensFact
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
        return JsonSerializer.Serialize(result, AppJsonContext.Default.MittensFact);
    }

    [McpServerTool]
    [Description("Delete a memory fact by ID. Use when the user says 'forget', 'remove', 'delete that memory', or wants something removed from storage.")]
    public async Task<string> MittensDelete(
        [Description("Fact ID to delete")] int id)
    {
        var result = await _sender.Send(new DeleteFact(id));
        return result ? "true" : "false";
    }
}
