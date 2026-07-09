namespace Mittens.Models;

public sealed record ValidationError(string Property, string Message, bool IsWarning = false);
