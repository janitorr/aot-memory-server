namespace AotMemoryServer.Models;

public class MemoryFact
{
    public int Id { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;
    public double Confidence { get; set; } = 1.0;
    public string? Source { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public bool IsDeprecated { get; set; }
}
