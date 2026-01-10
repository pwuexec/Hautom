using FluentResults;

namespace Hautom.Prompt.Models.Errors;

/// <summary>
/// Represents a validation error
/// </summary>
public sealed class ValidationError : Error
{
    public string PropertyName { get; }

    public ValidationError(string propertyName, string message) : base(message)
    {
        PropertyName = propertyName;
        Metadata.Add("PropertyName", propertyName);
    }
}
