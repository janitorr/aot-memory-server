using AotMemoryServer.Models;

namespace AotMemoryServer.Tests.Unit;

public sealed class MemoryFactValidatorTests
{
    [Fact]
    public void Validate_NullFact_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => MemoryFactValidator.Validate(null!));
    }

    [Fact]
    public void Validate_EmptyKey_ReturnsError()
    {
        var fact = new MemoryFact { Key = "", Value = "v", Category = "fact", Scope = "global" };
        var errors = MemoryFactValidator.Validate(fact);

        var error = Assert.Single(errors);
        Assert.Equal("Key", error.Property);
        Assert.False(error.IsWarning);
    }

    [Fact]
    public void Validate_EmptyValue_ReturnsError()
    {
        var fact = new MemoryFact { Key = "k", Value = "", Category = "fact", Scope = "global" };
        var errors = MemoryFactValidator.Validate(fact);

        var error = Assert.Single(errors);
        Assert.Equal("Value", error.Property);
        Assert.False(error.IsWarning);
    }

    [Fact]
    public void Validate_WhitespaceKey_ReturnsError()
    {
        var fact = new MemoryFact { Key = "   ", Value = "v", Category = "fact", Scope = "global" };
        var errors = MemoryFactValidator.Validate(fact);

        Assert.Contains(errors, e => e.Property == "Key" && !e.IsWarning);
    }

    [Fact]
    public void Validate_ValueExceedsMaxLength_ReturnsError()
    {
        var fact = new MemoryFact { Key = "k", Value = new string('x', 10_001), Category = "fact", Scope = "global" };
        var errors = MemoryFactValidator.Validate(fact);

        Assert.Contains(errors, e => e.Property == "Value" && !e.IsWarning);
    }

    [Fact]
    public void Validate_ValueAtMaxLength_ReturnsNoError()
    {
        var fact = new MemoryFact { Key = "k", Value = new string('x', 10_000), Category = "fact", Scope = "global" };
        var errors = MemoryFactValidator.Validate(fact);

        Assert.DoesNotContain(errors, e => e.Property == "Value");
    }

    [Fact]
    public void Validate_ValueContainsSecretKey_ReturnsError()
    {
        var fact = new MemoryFact { Key = "k", Value = "my key is sk-abc123def456ghi789jklmno", Category = "fact", Scope = "global" };
        var errors = MemoryFactValidator.Validate(fact);

        Assert.Contains(errors, e => e.Property == "Value" && !e.IsWarning);
    }

    [Fact]
    public void Validate_ValueContainsApiKey_ReturnsError()
    {
        var fact = new MemoryFact { Key = "k", Value = "use api_key=foobar here", Category = "fact", Scope = "global" };
        var errors = MemoryFactValidator.Validate(fact);

        Assert.Contains(errors, e => e.Property == "Value" && !e.IsWarning);
    }

    [Fact]
    public void Validate_ValueContainsApiKeyUnderscore_ReturnsError()
    {
        var fact = new MemoryFact { Key = "k", Value = "API_KEY=12345", Category = "fact", Scope = "global" };
        var errors = MemoryFactValidator.Validate(fact);

        Assert.Contains(errors, e => e.Property == "Value" && !e.IsWarning);
    }

    [Fact]
    public void Validate_ValueContainsSecret_ReturnsError()
    {
        var fact = new MemoryFact { Key = "k", Value = "the secret is xyz", Category = "fact", Scope = "global" };
        var errors = MemoryFactValidator.Validate(fact);

        Assert.Contains(errors, e => e.Property == "Value" && !e.IsWarning);
    }

    [Fact]
    public void Validate_ValueContainsToken_ReturnsError()
    {
        var fact = new MemoryFact { Key = "k", Value = "token=abc123", Category = "fact", Scope = "global" };
        var errors = MemoryFactValidator.Validate(fact);

        Assert.Contains(errors, e => e.Property == "Value" && !e.IsWarning);
    }

    [Fact]
    public void Validate_ValueContainsPassword_ReturnsError()
    {
        var fact = new MemoryFact { Key = "k", Value = "password=hunter2", Category = "fact", Scope = "global" };
        var errors = MemoryFactValidator.Validate(fact);

        Assert.Contains(errors, e => e.Property == "Value" && !e.IsWarning);
    }

    [Fact]
    public void Validate_ValueContainsPrivateKeyHeader_ReturnsError()
    {
        var fact = new MemoryFact { Key = "k", Value = "-----BEGIN RSA PRIVATE KEY-----\nABC123", Category = "fact", Scope = "global" };
        var errors = MemoryFactValidator.Validate(fact);

        Assert.Contains(errors, e => e.Property == "Value" && !e.IsWarning);
    }

    [Fact]
    public void Validate_ValueContainsCSharpCode_ReturnsWarning()
    {
        var fact = new MemoryFact { Key = "k", Value = "using System; class Foo { }", Category = "fact", Scope = "global" };
        var errors = MemoryFactValidator.Validate(fact);

        Assert.Contains(errors, e => e.Property == "Value" && e.IsWarning);
    }

    [Fact]
    public void Validate_ValueContainsJavaScriptCode_ReturnsWarning()
    {
        var fact = new MemoryFact { Key = "k", Value = "function hello() { return 1; }", Category = "fact", Scope = "global" };
        var errors = MemoryFactValidator.Validate(fact);

        Assert.Contains(errors, e => e.Property == "Value" && e.IsWarning);
    }

    [Fact]
    public void Validate_ValueContainsPythonCode_ReturnsWarning()
    {
        var fact = new MemoryFact { Key = "k", Value = "def hello():\n    print('hi')", Category = "fact", Scope = "global" };
        var errors = MemoryFactValidator.Validate(fact);

        Assert.Contains(errors, e => e.Property == "Value" && e.IsWarning);
    }

    [Fact]
    public void Validate_EmptyCategory_ReturnsError()
    {
        var fact = new MemoryFact { Key = "k", Value = "v", Category = "", Scope = "global" };
        var errors = MemoryFactValidator.Validate(fact);

        Assert.Contains(errors, e => e.Property == "Category" && !e.IsWarning);
    }

    [Fact]
    public void Validate_UnknownCategory_ReturnsWarning()
    {
        var fact = new MemoryFact { Key = "k", Value = "v", Category = "unknown-category", Scope = "global" };
        var errors = MemoryFactValidator.Validate(fact);

        Assert.Contains(errors, e => e.Property == "Category" && e.IsWarning);
    }

    [Theory]
    [InlineData("preference")]
    [InlineData("fact")]
    [InlineData("concept")]
    [InlineData("rule")]
    [InlineData("plan")]
    [InlineData("goal")]
    [InlineData("task")]
    [InlineData("note")]
    public void Validate_KnownCategory_ReturnsNoCategoryError(string category)
    {
        var fact = new MemoryFact { Key = "k", Value = "v", Category = category, Scope = "global" };
        var errors = MemoryFactValidator.Validate(fact);

        Assert.DoesNotContain(errors, e => e.Property == "Category");
    }

    [Fact]
    public void Validate_ValidFact_ReturnsNoErrors()
    {
        var fact = new MemoryFact
        {
            Key = "test-key",
            Value = "test value",
            Category = "fact",
            Scope = "global",
            Confidence = 1.0
        };
        var errors = MemoryFactValidator.Validate(fact);

        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_MultipleErrors_ReturnsAll()
    {
        var fact = new MemoryFact { Key = "", Value = "my token is sk-test123", Category = "fact", Scope = "global" };
        var errors = MemoryFactValidator.Validate(fact);

        Assert.Contains(errors, e => e.Property == "Key");
        Assert.Contains(errors, e => e.Property == "Value" && !e.IsWarning);
    }

    [Fact]
    public void Validate_WarningOnly_DoesNotContainHardErrors()
    {
        var fact = new MemoryFact { Key = "k", Value = "using System; class Foo { }", Category = "fact", Scope = "global" };
        var errors = MemoryFactValidator.Validate(fact);

        Assert.All(errors, e => Assert.True(e.IsWarning));
    }
}
