using System.Text.Json.Serialization;
using AotMemoryServer.Application.Abstractions;
using AotMemoryServer.Models;

namespace AotMemoryServer.Application.Serialization;

[JsonSerializable(typeof(MemoryFact))]
[JsonSerializable(typeof(PagedResult<MemoryFact>))]
[JsonSerializable(typeof(List<MemoryFact>))]
[JsonSerializable(typeof(ValidationError))]
[JsonSerializable(typeof(HealthStatus))]
[JsonSerializable(typeof(ErrorResponse))]
public sealed partial class AppJsonContext : JsonSerializerContext
{
}
