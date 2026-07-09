using System.Text.Json.Serialization;
using Mittens.Application.Abstractions;
using Mittens.Models;

namespace Mittens.Application.Serialization;

[JsonSerializable(typeof(MittensFact))]
[JsonSerializable(typeof(PagedResult<MittensFact>))]
[JsonSerializable(typeof(List<MittensFact>))]
[JsonSerializable(typeof(ValidationError))]
[JsonSerializable(typeof(HealthStatus))]
[JsonSerializable(typeof(ErrorResponse))]
public sealed partial class AppJsonContext : JsonSerializerContext
{
}
