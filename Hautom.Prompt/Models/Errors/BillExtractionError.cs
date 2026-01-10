using FluentResults;

namespace Hautom.Prompt.Models.Errors;

/// <summary>
/// Represents an error that occurred during bill extraction
/// </summary>
public sealed class BillExtractionError : Error
{
    public string? FilePath { get; }

    public BillExtractionError(string message) : base(message)
    {
    }

    public BillExtractionError(string message, string filePath) : base(message)
    {
        FilePath = filePath;
        Metadata.Add("FilePath", filePath);
    }

    public BillExtractionError WithInnerException(Exception exception)
    {
        CausedBy(exception);
        return this;
    }
}
