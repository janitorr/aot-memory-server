using Mittens.Models;

namespace Mittens.Application.Abstractions;

public sealed class ValidationException(IReadOnlyList<ValidationError> errors)
    : Exception("One or more validation errors occurred.")
{
    public IReadOnlyList<ValidationError> Errors { get; } = errors;
}
