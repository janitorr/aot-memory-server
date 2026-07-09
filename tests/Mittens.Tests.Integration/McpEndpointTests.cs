using System.Text;
using System.Text.Json;
using Mittens.Models;

namespace Mittens.Tests.Integration;

[Collection("IntegrationTests")]
public sealed class McpEndpointTests : IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private int _requestId;

    public McpEndpointTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Accept.Add(
            new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        _client.DefaultRequestHeaders.Accept.Add(
            new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/event-stream"));
    }

    public Task InitializeAsync() => _factory.ClearFactsAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task ToolsList_ReturnsAllTools()
    {
        var response = await SendRequest("tools/list", new { });
        var doc = await ParseResponse(response);

        var tools = doc.RootElement.GetProperty("result").GetProperty("tools");
        var names = tools.EnumerateArray().Select(t => t.GetProperty("name").GetString()).ToHashSet();

        Assert.Contains("mittens_list", names);
        Assert.Contains("mittens_get", names);
        Assert.Contains("mittens_search", names);
        Assert.Contains("mittens_set", names);
        Assert.Contains("mittens_update", names);
        Assert.Contains("mittens_delete", names);
    }

    [Fact]
    public async Task MemorySet_Valid_ReturnsFact()
    {
        var response = await SendToolCall("mittens_set", new
        {
            key = "mcp-key",
            value = "mcp value",
            category = "fact",
            scope = "global",
            confidence = 1.0
        });
        var result = await GetToolResult(response);

        Assert.Contains("mcp-key", result);
        Assert.Contains("mcp value", result);
    }

    [Fact]
    public async Task MemorySet_Invalid_ReturnsError()
    {
        var response = await SendToolCall("mittens_set", new
        {
            key = "",
            value = "v",
            category = "fact",
            scope = "global"
        });
        var doc = await ParseResponse(response);

        Assert.True(doc.RootElement.TryGetProperty("error", out _) ||
                    doc.RootElement.GetProperty("result").GetProperty("isError").GetBoolean());
    }

    [Fact]
    public async Task MemoryList_ReturnsFacts()
    {
        await _factory.CreateFactAsync(new MittensFact
        {
            Key = "mk1", Value = "mv1", Category = "fact", Scope = "global", Confidence = 1.0
        });
        await _factory.CreateFactAsync(new MittensFact
        {
            Key = "mk2", Value = "mv2", Category = "fact", Scope = "global", Confidence = 1.0
        });

        var response = await SendToolCall("mittens_list", new { });
        var result = await GetToolResult(response);

        Assert.Contains("mk1", result);
        Assert.Contains("mk2", result);
    }

    [Fact]
    public async Task MemoryGet_Existing_ReturnsFact()
    {
        var created = await _factory.CreateFactAsync(new MittensFact
        {
            Key = "get-key", Value = "get-val", Category = "fact", Scope = "global", Confidence = 1.0
        });

        var response = await SendToolCall("mittens_get", new { id = created.Id });
        var result = await GetToolResult(response);

        Assert.Contains("get-key", result);
        Assert.Contains("get-val", result);
    }

    [Fact]
    public async Task MemoryGet_NonExistent_ReturnsNull()
    {
        var response = await SendToolCall("mittens_get", new { id = 999 });
        var result = await GetToolResult(response);

        Assert.Equal("null", result);
    }

    [Fact]
    public async Task MemorySearch_ReturnsMatches()
    {
        await _factory.CreateFactAsync(new MittensFact
        {
            Key = "apple", Value = "fruit", Category = "fact", Scope = "global", Confidence = 1.0
        });
        await _factory.CreateFactAsync(new MittensFact
        {
            Key = "banana", Value = "yellow fruit", Category = "fact", Scope = "global", Confidence = 1.0
        });

        var response = await SendToolCall("mittens_search", new { q = "fruit" });
        var result = await GetToolResult(response);

        Assert.Contains("apple", result);
        Assert.Contains("banana", result);
    }

    [Fact]
    public async Task MemoryUpdate_Existing_Updates()
    {
        var created = await _factory.CreateFactAsync(new MittensFact
        {
            Key = "upd-key", Value = "old", Category = "fact", Scope = "global", Confidence = 1.0
        });

        var response = await SendToolCall("mittens_update", new
        {
            id = created.Id,
            value = "new",
            category = "rule"
        });
        var result = await GetToolResult(response);

        Assert.Contains("new", result);
    }

    [Fact]
    public async Task MemoryUpdate_NonExistent_ReturnsError()
    {
        var response = await SendToolCall("mittens_update", new
        {
            id = 999,
            value = "new",
            category = "fact"
        });
        var result = await GetToolResult(response);
        Assert.Contains("not found", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task MemoryDelete_Existing_ReturnsTrue()
    {
        var created = await _factory.CreateFactAsync(new MittensFact
        {
            Key = "del-key", Value = "v", Category = "fact", Scope = "global", Confidence = 1.0
        });

        var response = await SendToolCall("mittens_delete", new { id = created.Id });
        var result = await GetToolResult(response);

        Assert.Equal("true", result);
    }

    [Fact]
    public async Task MemoryDelete_NonExistent_ReturnsFalse()
    {
        var response = await SendToolCall("mittens_delete", new { id = 999 });
        var result = await GetToolResult(response);

        Assert.Equal("false", result);
    }

    private async Task<HttpResponseMessage> SendRequest(string method, object? paramsObj)
    {
        var id = Interlocked.Increment(ref _requestId);
        var body = JsonSerializer.Serialize(new
        {
            jsonrpc = "2.0",
            id,
            method,
            @params = paramsObj
        });
        var content = new StringContent(body, Encoding.UTF8, "application/json");
        return await _client.PostAsync("/mcp", content);
    }

    private Task<HttpResponseMessage> SendToolCall(string tool, object args)
        => SendRequest("tools/call", new { name = tool, arguments = args });

    private static async Task<JsonDocument> ParseResponse(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();

        // Handle SSE format: "event: message\ndata: {...}\n\n"
        if (body.StartsWith("event:", StringComparison.Ordinal))
        {
            foreach (var line in body.Split('\n'))
            {
                if (line.StartsWith("data: ", StringComparison.Ordinal))
                {
                    return JsonDocument.Parse(line[6..]);
                }
            }
        }

        return JsonDocument.Parse(body);
    }

    private static async Task<string> GetToolResult(HttpResponseMessage response)
    {
        var doc = await ParseResponse(response);
        return doc.RootElement.GetProperty("result").GetProperty("content")[0].GetProperty("text").GetString()!;
    }
}
