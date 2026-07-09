using System.Text.RegularExpressions;

namespace Mittens.Models;

public static partial class MittensFactValidator
{
    private const int MaxValueLength = 10_000;

    private static readonly HashSet<string> KnownCategories = new(StringComparer.OrdinalIgnoreCase)
    {
        "preference", "fact", "concept", "rule", "plan", "goal", "task", "note"
    };

    [GeneratedRegex(@"\b(?i)(sk-[a-zA-Z0-9]{20,}|api[_-]?key|secret|token|password|private\s*key|-----BEGIN\s+.*KEY-----)",
        RegexOptions.Compiled)]
    private static partial Regex SecretPattern();

    public static IReadOnlyList<ValidationError> Validate(MittensFact fact)
    {
        ArgumentNullException.ThrowIfNull(fact);

        List<ValidationError> errors = [];

        AddIfNotNull(errors, ValidateRequired(fact.Key, nameof(fact.Key)));
        AddIfNotNull(errors, ValidateRequired(fact.Value, nameof(fact.Value)));
        AddIfNotNull(errors, ValidateLength(fact.Value, nameof(fact.Value)));
        AddIfNotNull(errors, ValidateNoSecrets(fact.Value, nameof(fact.Value)));
        AddIfNotNull(errors, ValidateCategory(fact.Category));

        return errors;
    }

    public static MittensFact ResolveConflict(MittensFact existing, MittensFact incoming, bool force = false)
    {
        ArgumentNullException.ThrowIfNull(existing);
        ArgumentNullException.ThrowIfNull(incoming);

        if (force || incoming.Confidence > existing.Confidence)
        {
            incoming.Id = existing.Id;
            return incoming;
        }

        return existing;
    }

    private static void AddIfNotNull(List<ValidationError> list, ValidationError? error)
    {
        if (error is not null)
        {
            list.Add(error);
        }
    }

    private static ValidationError? ValidateRequired(string value, string propertyName)
    {
        return string.IsNullOrWhiteSpace(value)
            ? new ValidationError(propertyName, $"{propertyName} must not be empty.")
            : null;
    }

    private static ValidationError? ValidateLength(string value, string propertyName)
    {
        return value.Length > MaxValueLength
            ? new ValidationError(propertyName, $"{propertyName} exceeds {MaxValueLength} character limit.")
            : null;
    }

    private static ValidationError? ValidateNoSecrets(string value, string propertyName)
    {
        return SecretPattern().IsMatch(value)
            ? new ValidationError(propertyName, "Value appears to contain secrets, API keys, or credentials.")
            : null;
    }

    private static ValidationError? ValidateCategory(string category)
    {
        if (string.IsNullOrWhiteSpace(category))
        {
            return new ValidationError(nameof(MittensFact.Category), "Category must not be empty.");
        }

        if (!KnownCategories.Contains(category))
        {
            return new ValidationError(nameof(MittensFact.Category),
                $"Unknown category '{category}'. Known: {string.Join(", ", KnownCategories)}.", IsWarning: true);
        }

        return null;
    }
}
