using Mittens.Core.Fact;

namespace Mittens.Host.Serialization;

public sealed record HealthStatus(string Status);

public sealed record ErrorResponse(IReadOnlyList<ValidationError> Errors);
