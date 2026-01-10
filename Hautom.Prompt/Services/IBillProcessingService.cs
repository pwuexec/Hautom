using FluentResults;

namespace Hautom.Prompt.Services;

/// <summary>
/// Service for batch processing electricity bill PDFs
/// </summary>
public interface IBillProcessingService
{
    /// <summary>
    /// Processes all PDF files in the specified folder
    /// </summary>
    /// <param name="folderPath">Path to folder containing PDF files</param>
    /// <param name="filePattern">File pattern to match (default: *.pdf)</param>
    /// <returns>Result containing processing statistics</returns>
    Result<ProcessingResult> ProcessFolder(string folderPath, string filePattern = "*.pdf");
}

/// <summary>
/// Result of a batch processing operation
/// </summary>
public sealed record ProcessingResult
{
    public required int FilesFound { get; init; }
    public required int Processed { get; init; }
    public required int Skipped { get; init; }
    public required int Errors { get; init; }
    public required IReadOnlyList<string> ErrorMessages { get; init; }
}
