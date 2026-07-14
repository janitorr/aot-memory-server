using System.Text.Json.Serialization;
using Mittens.Core.Fact;
using Mittens.Core.Shared;
using Fact = Mittens.Core.Fact.Fact;

namespace Mittens.Host.Serialization;

[JsonSerializable(typeof(Fact))]
[JsonSerializable(typeof(PagedResult<Fact>))]
[JsonSerializable(typeof(List<Fact>))]
[JsonSerializable(typeof(ValidationError))]
[JsonSerializable(typeof(HealthStatus))]
[JsonSerializable(typeof(ErrorResponse))]
public sealed partial class AppJsonContext : JsonSerializerContext
{
}
