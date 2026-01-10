using FluentResults;
using Hautom.Prompt.Models.Errors;

namespace Hautom.Prompt.Configuration;

/// <summary>
/// Application configuration settings
/// </summary>
public sealed class AppConfiguration
{
    public required string FolderPath { get; init; }
    public string FilePattern { get; init; } = "*.pdf";
    public bool ExportToJson { get; init; } = true;
    public string OutputFolder { get; init; } = string.Empty;
    public required string DatabasePath { get; init; }

    /// <summary>
    /// Validates the configuration
    /// </summary>
    public Result Validate()
    {
        if (string.IsNullOrWhiteSpace(FolderPath))
            return Result.Fail(new ValidationError(nameof(FolderPath), "Folder path cannot be empty"));

        if (!Directory.Exists(FolderPath))
            return Result.Fail(new ValidationError(nameof(FolderPath), $"Directory not found: {FolderPath}"));

        if (string.IsNullOrWhiteSpace(DatabasePath))
            return Result.Fail(new ValidationError(nameof(DatabasePath), "Database path cannot be empty"));

        return Result.Ok();
    }
}
