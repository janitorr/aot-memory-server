using Mittens.Models;

namespace Mittens.Tests.Unit;

public sealed class ConflictResolutionTests
{
    [Fact]
    public void ResolveConflict_NullExisting_Throws()
    {
        var incoming = new MittensFact();
        Assert.Throws<ArgumentNullException>(() => MittensFactValidator.ResolveConflict(null!, incoming));
    }

    [Fact]
    public void ResolveConflict_NullIncoming_Throws()
    {
        var existing = new MittensFact();
        Assert.Throws<ArgumentNullException>(() => MittensFactValidator.ResolveConflict(existing, null!));
    }

    [Fact]
    public void ResolveConflict_IncomingHigherConfidence_Wins()
    {
        var existing = new MittensFact { Id = 1, Key = "k", Value = "old", Confidence = 0.5 };
        var incoming = new MittensFact { Id = 0, Key = "k", Value = "new", Confidence = 0.9 };

        var result = MittensFactValidator.ResolveConflict(existing, incoming);

        Assert.Equal("new", result.Value);
        Assert.Equal(1, result.Id);
    }

    [Fact]
    public void ResolveConflict_IncomingLowerConfidence_Loses()
    {
        var existing = new MittensFact { Id = 1, Key = "k", Value = "old", Confidence = 0.9 };
        var incoming = new MittensFact { Id = 0, Key = "k", Value = "new", Confidence = 0.5 };

        var result = MittensFactValidator.ResolveConflict(existing, incoming);

        Assert.Equal("old", result.Value);
        Assert.Equal(1, result.Id);
    }

    [Fact]
    public void ResolveConflict_IncomingEqualConfidence_Loses()
    {
        var existing = new MittensFact { Id = 1, Key = "k", Value = "old", Confidence = 0.7 };
        var incoming = new MittensFact { Id = 0, Key = "k", Value = "new", Confidence = 0.7 };

        var result = MittensFactValidator.ResolveConflict(existing, incoming);

        Assert.Equal("old", result.Value);
    }

    [Fact]
    public void ResolveConflict_ForceFlag_WinsRegardlessOfConfidence()
    {
        var existing = new MittensFact { Id = 1, Key = "k", Value = "old", Confidence = 0.9 };
        var incoming = new MittensFact { Id = 0, Key = "k", Value = "new", Confidence = 0.1 };

        var result = MittensFactValidator.ResolveConflict(existing, incoming, force: true);

        Assert.Equal("new", result.Value);
        Assert.Equal(1, result.Id);
    }

    [Fact]
    public void ResolveConflict_ForceFlagFalse_LosesOnLowerConfidence()
    {
        var existing = new MittensFact { Id = 1, Key = "k", Value = "old", Confidence = 0.9 };
        var incoming = new MittensFact { Id = 0, Key = "k", Value = "new", Confidence = 0.1 };

        var result = MittensFactValidator.ResolveConflict(existing, incoming, force: false);

        Assert.Equal("old", result.Value);
    }

    [Fact]
    public void ResolveConflict_IncomingPreservesExistingId()
    {
        var existing = new MittensFact { Id = 42, Key = "k", Value = "old", Confidence = 0.5 };
        var incoming = new MittensFact { Id = 99, Key = "k", Value = "new", Confidence = 0.9 };

        var result = MittensFactValidator.ResolveConflict(existing, incoming);

        Assert.Equal(42, result.Id);
    }
}
