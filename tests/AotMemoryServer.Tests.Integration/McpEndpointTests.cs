using System.Net;
using System.Text;
using System.Text.Json;
using AotMemoryServer.Models;

namespace AotMemoryServer.Tests.Integration;

public sealed class McpEndpointTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public McpEndpointTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public Task InitializeAsync() => _factory.ClearFactsAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task InvalidJson_ReturnsParseError()
    {
        var response = await PostJsonAsync("not json");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var doc = await ParseResponse(response);
        Assert.Equal(-32700, doc.RootElement.GetProperty("error").GetProperty("code").GetInt32());
    }

    [Fact]
    public async Task MissingVersion_ReturnsInvalidRequest()
    {
        var body = JsonSerializer.Serialize(new { method = "memory/list", id = 1 });
        var response = await PostJsonAsync(body);

        var doc = await ParseResponse(response);
        Assert.Equal(-32600, doc.RootElement.GetProperty("error").GetProperty("code").GetInt32());
    }

    [Fact]
    public async Task UnknownMethod_ReturnsMethodNotFound()
    {
        var body = JsonSerializer.Serialize(new
        {
            jsonrpc = "2.0",
            method = "bogus",
            id = 1
        });
        var response = await PostJsonAsync(body);

        var doc = await ParseResponse(response);
        Assert.Equal(-32601, doc.RootElement.GetProperty("error").GetProperty("code").GetInt32());
    }

    [Fact]
    public async Task MemorySet_Valid_ReturnsFact()
    {
        var body = JsonSerializer.Serialize(new
        {
            jsonrpc = "2.0",
            method = "memory/set",
            id = 1,
            @params = new
            {
                fact = new
                {
                    key = "mcp-key",
                    value = "mcp value",
                    category = "fact",
                    scope = "global",
                    confidence = 1.0
                }
            }
        });

        var response = await PostJsonAsync(body);
        var doc = await ParseResponse(response);

        Assert.Equal("2.0", doc.RootElement.GetProperty("jsonrpc").GetString());
        Assert.Equal(1, doc.RootElement.GetProperty("id").GetInt32());
        Assert.Equal("mcp-key", doc.RootElement.GetProperty("result").GetProperty("Key").GetString());
    }

    [Fact]
    public async Task MemorySet_Invalid_ReturnsError()
    {
        var body = JsonSerializer.Serialize(new
        {
            jsonrpc = "2.0",
            method = "memory/set",
            id = 1,
            @params = new
            {
                fact = new
                {
                    key = "",
                    value = "v",
                    category = "fact",
                    scope = "global"
                }
            }
        });

        var response = await PostJsonAsync(body);
        var doc = await ParseResponse(response);

        Assert.Equal(-32000, doc.RootElement.GetProperty("error").GetProperty("code").GetInt32());
    }

    [Fact]
    public async Task MemoryList_ReturnsFacts()
    {
        await _factory.CreateFactAsync(new MemoryFact
        {
            Key = "mk1", Value = "mv1", Category = "fact", Scope = "global", Confidence = 1.0
        });
        await _factory.CreateFactAsync(new MemoryFact
        {
            Key = "mk2", Value = "mv2", Category = "fact", Scope = "global", Confidence = 1.0
        });

        var body = JsonSerializer.Serialize(new
        {
            jsonrpc = "2.0",
            method = "memory/list",
            id = 1
        });

        var response = await PostJsonAsync(body);
        var doc = await ParseResponse(response);

        var totalCount = doc.RootElement.GetProperty("result").GetProperty("TotalCount").GetInt32();
        Assert.Equal(2, totalCount);
    }

    [Fact]
    public async Task MemoryGet_Existing_ReturnsFact()
    {
        var created = await _factory.CreateFactAsync(new MemoryFact
        {
            Key = "get-key", Value = "get-val", Category = "fact", Scope = "global", Confidence = 1.0
        });

        var body = JsonSerializer.Serialize(new
        {
            jsonrpc = "2.0",
            method = "memory/get",
            id = 1,
            @params = new { id = created.Id }
        });

        var response = await PostJsonAsync(body);
        var doc = await ParseResponse(response);

        Assert.Equal("get-key", doc.RootElement.GetProperty("result").GetProperty("Key").GetString());
    }

    [Fact]
    public async Task MemoryGet_NonExistent_ReturnsNullResult()
    {
        var body = JsonSerializer.Serialize(new
        {
            jsonrpc = "2.0",
            method = "memory/get",
            id = 1,
            @params = new { id = 999 }
        });

        var response = await PostJsonAsync(body);
        var doc = await ParseResponse(response);

        Assert.Equal(JsonValueKind.Null, doc.RootElement.GetProperty("result").ValueKind);
    }

    [Fact]
    public async Task MemorySearch_ReturnsMatches()
    {
        await _factory.CreateFactAsync(new MemoryFact
        {
            Key = "apple", Value = "fruit", Category = "fact", Scope = "global", Confidence = 1.0
        });
        await _factory.CreateFactAsync(new MemoryFact
        {
            Key = "banana", Value = "yellow fruit", Category = "fact", Scope = "global", Confidence = 1.0
        });
        await _factory.CreateFactAsync(new MemoryFact
        {
            Key = "carrot", Value = "vegetable", Category = "fact", Scope = "global", Confidence = 1.0
        });

        var body = JsonSerializer.Serialize(new
        {
            jsonrpc = "2.0",
            method = "memory/search",
            id = 1,
            @params = new { q = "fruit" }
        });

        var response = await PostJsonAsync(body);
        var doc = await ParseResponse(response);

        var items = doc.RootElement.GetProperty("result").GetProperty("Items");
        Assert.Equal(2, items.GetArrayLength());
    }

    [Fact]
    public async Task MemoryUpdate_Existing_Updates()
    {
        var created = await _factory.CreateFactAsync(new MemoryFact
        {
            Key = "upd-key", Value = "old", Category = "fact", Scope = "global", Confidence = 1.0
        });

        var body = JsonSerializer.Serialize(new
        {
            jsonrpc = "2.0",
            method = "memory/update",
            id = 1,
            @params = new
            {
                id = created.Id,
                fact = new
                {
                    key = "upd-key",
                    value = "new",
                    category = "rule",
                    scope = "global",
                    confidence = 0.5
                }
            }
        });

        var response = await PostJsonAsync(body);
        var doc = await ParseResponse(response);

        Assert.Equal("new", doc.RootElement.GetProperty("result").GetProperty("Value").GetString());
    }

    [Fact]
    public async Task MemoryUpdate_NonExistent_ReturnsNullResult()
    {
        var body = JsonSerializer.Serialize(new
        {
            jsonrpc = "2.0",
            method = "memory/update",
            id = 1,
            @params = new
            {
                id = 999,
                fact = new
                {
                    key = "k",
                    value = "v",
                    category = "fact",
                    scope = "global",
                    confidence = 1.0
                }
            }
        });

        var response = await PostJsonAsync(body);
        var doc = await ParseResponse(response);

        Assert.Equal(JsonValueKind.Null, doc.RootElement.GetProperty("result").ValueKind);
    }

    [Fact]
    public async Task MemoryDelete_Existing_ReturnsTrue()
    {
        var created = await _factory.CreateFactAsync(new MemoryFact
        {
            Key = "del-key", Value = "v", Category = "fact", Scope = "global", Confidence = 1.0
        });

        var body = JsonSerializer.Serialize(new
        {
            jsonrpc = "2.0",
            method = "memory/delete",
            id = 1,
            @params = new { id = created.Id }
        });

        var response = await PostJsonAsync(body);
        var doc = await ParseResponse(response);

        Assert.True(doc.RootElement.GetProperty("result").GetBoolean());
    }

    [Fact]
    public async Task MemoryDelete_NonExistent_ReturnsFalse()
    {
        var body = JsonSerializer.Serialize(new
        {
            jsonrpc = "2.0",
            method = "memory/delete",
            id = 1,
            @params = new { id = 999 }
        });

        var response = await PostJsonAsync(body);
        var doc = await ParseResponse(response);

        Assert.False(doc.RootElement.GetProperty("result").GetBoolean());
    }

    [Fact]
    public async Task Notification_NoId_ReturnsEmpty()
    {
        var body = JsonSerializer.Serialize(new
        {
            jsonrpc = "2.0",
            method = "memory/list"
        });

        var response = await PostJsonAsync(body);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var bodyText = await response.Content.ReadAsStringAsync();
        Assert.Empty(bodyText);
    }

    private async Task<HttpResponseMessage> PostJsonAsync(string json)
    {
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        return await _client.PostAsync("/api/mcp", content);
    }

    private static async Task<JsonDocument> ParseResponse(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        return JsonDocument.Parse(json);
    }
}
