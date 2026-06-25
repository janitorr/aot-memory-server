using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AotMemoryServer.Application.Abstractions;
using AotMemoryServer.Application.Serialization;
using AotMemoryServer.Models;

namespace AotMemoryServer.Tests.Integration;

public sealed class RestEndpointTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public RestEndpointTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public Task InitializeAsync() => _factory.ClearFactsAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetFacts_NoData_ReturnsEmptyList()
    {
        var response = await _client.GetAsync("/api/memory/");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<PagedResult<MemoryFact>>(JsonOptions);
        Assert.NotNull(result);
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
    }

    [Fact]
    public async Task GetFacts_WithData_ReturnsAll()
    {
        await _factory.CreateFactAsync(new MemoryFact
        {
            Key = "k1", Value = "v1", Category = "fact", Scope = "global", Confidence = 1.0
        });
        await _factory.CreateFactAsync(new MemoryFact
        {
            Key = "k2", Value = "v2", Category = "fact", Scope = "global", Confidence = 1.0
        });

        var response = await _client.GetAsync("/api/memory/");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<PagedResult<MemoryFact>>(JsonOptions);
        Assert.NotNull(result);
        Assert.Equal(2, result.TotalCount);
    }

    [Fact]
    public async Task GetFacts_WithCategoryFilter_ReturnsFiltered()
    {
        await _factory.CreateFactAsync(new MemoryFact
        {
            Key = "k1", Value = "v1", Category = "fact", Scope = "global", Confidence = 1.0
        });
        await _factory.CreateFactAsync(new MemoryFact
        {
            Key = "k2", Value = "v2", Category = "preference", Scope = "global", Confidence = 1.0
        });

        var response = await _client.GetAsync("/api/memory/?category=fact");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<PagedResult<MemoryFact>>(JsonOptions);
        Assert.NotNull(result);
        Assert.Equal(1, result.TotalCount);
        Assert.All(result.Items, f => Assert.Equal("fact", f.Category));
    }

    [Fact]
    public async Task GetFactById_Existing_ReturnsFact()
    {
        var created = await _factory.CreateFactAsync(new MemoryFact
        {
            Key = "k1", Value = "v1", Category = "fact", Scope = "global", Confidence = 1.0
        });

        var response = await _client.GetAsync($"/api/memory/{created.Id}");
        response.EnsureSuccessStatusCode();

        var fact = await response.Content.ReadFromJsonAsync<MemoryFact>(JsonOptions);
        Assert.NotNull(fact);
        Assert.Equal(created.Id, fact.Id);
        Assert.Equal("k1", fact.Key);
    }

    [Fact]
    public async Task GetFactById_NonExistent_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/memory/999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PostFact_Valid_ReturnsCreatedFact()
    {
        var fact = new MemoryFact
        {
            Key = "new-key",
            Value = "new value",
            Category = "fact",
            Scope = "global",
            Confidence = 1.0
        };

        var response = await _client.PostAsJsonAsync("/api/memory/", fact);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<MemoryFact>(JsonOptions);
        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.Equal("new-key", result.Key);
        Assert.NotEmpty(result.UpdatedAt);
    }

    [Fact]
    public async Task PostFact_Invalid_ReturnsBadRequest()
    {
        var fact = new MemoryFact
        {
            Key = "",
            Value = "some value",
            Category = "fact",
            Scope = "global",
            Confidence = 1.0
        };

        var response = await _client.PostAsJsonAsync("/api/memory/", fact);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(JsonOptions);
        Assert.NotNull(error);
        Assert.NotEmpty(error.Errors);
    }

    [Fact]
    public async Task PostFact_Conflict_LowerConfidence_KeepsExisting()
    {
        await _factory.CreateFactAsync(new MemoryFact
        {
            Key = "dup", Value = "original", Category = "fact", Scope = "global", Confidence = 1.0
        });

        var incoming = new MemoryFact
        {
            Key = "dup", Value = "newer", Category = "fact", Scope = "global", Confidence = 0.5
        };

        var response = await _client.PostAsJsonAsync("/api/memory/", incoming);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<MemoryFact>(JsonOptions);
        Assert.NotNull(result);
        Assert.Equal("original", result.Value);
    }

    [Fact]
    public async Task PostFact_Conflict_ForceFlag_Overwrites()
    {
        await _factory.CreateFactAsync(new MemoryFact
        {
            Key = "dup", Value = "original", Category = "fact", Scope = "global", Confidence = 1.0
        });

        var incoming = new MemoryFact
        {
            Key = "dup", Value = "overwritten", Category = "fact", Scope = "global", Confidence = 0.5
        };

        var response = await _client.PostAsJsonAsync("/api/memory/?force=true", incoming);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<MemoryFact>(JsonOptions);
        Assert.NotNull(result);
        Assert.Equal("overwritten", result.Value);
    }

    [Fact]
    public async Task PutFact_Existing_Updates()
    {
        var created = await _factory.CreateFactAsync(new MemoryFact
        {
            Key = "k1", Value = "original", Category = "fact", Scope = "global", Confidence = 1.0
        });

        var update = new MemoryFact
        {
            Key = "k1", Value = "updated", Category = "rule", Scope = "global", Confidence = 0.8
        };

        var response = await _client.PutAsJsonAsync($"/api/memory/{created.Id}", update);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<MemoryFact>(JsonOptions);
        Assert.NotNull(result);
        Assert.Equal("updated", result.Value);
        Assert.Equal("rule", result.Category);
    }

    [Fact]
    public async Task PutFact_NonExistent_ReturnsNotFound()
    {
        var fact = new MemoryFact
        {
            Key = "k", Value = "v", Category = "fact", Scope = "global", Confidence = 1.0
        };

        var response = await _client.PutAsJsonAsync("/api/memory/999", fact);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PutFact_Invalid_ReturnsBadRequest()
    {
        var created = await _factory.CreateFactAsync(new MemoryFact
        {
            Key = "k1", Value = "v1", Category = "fact", Scope = "global", Confidence = 1.0
        });

        var invalid = new MemoryFact
        {
            Key = "k1", Value = "", Category = "fact", Scope = "global", Confidence = 1.0
        };

        var response = await _client.PutAsJsonAsync($"/api/memory/{created.Id}", invalid);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(JsonOptions);
        Assert.NotNull(error);
        Assert.NotEmpty(error.Errors);
    }

    [Fact]
    public async Task DeleteFact_Existing_ReturnsNoContent()
    {
        var created = await _factory.CreateFactAsync(new MemoryFact
        {
            Key = "k1", Value = "v1", Category = "fact", Scope = "global", Confidence = 1.0
        });

        var response = await _client.DeleteAsync($"/api/memory/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteFact_NonExistent_ReturnsNotFound()
    {
        var response = await _client.DeleteAsync("/api/memory/999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
