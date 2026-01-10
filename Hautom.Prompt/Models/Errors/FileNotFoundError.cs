using FluentResults;

namespace Hautom.Prompt.Models.Errors;

/// <summary>
/// Represents a file not found error
/// </summary>
public sealed class FileNotFoundError(string filePath) : Error($"File not found: {filePath}")
{
    public string FilePath { get; } = filePath;
}
