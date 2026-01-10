namespace Hautom.Api.Configuration;

/// <summary>
/// Configuration options for bill processing background job
/// </summary>
public sealed class BillProcessingOptions
{
    public const string SectionName = "BillProcessing";

    /// <summary>
    /// List of folder paths containing PDF bills to process
    /// </summary>
    public List<string> FolderPaths { get; set; } = [];

    /// <summary>
    /// File pattern to match (default: *.pdf)
    /// </summary>
    public string FilePattern { get; set; } = "*.pdf";

    /// <summary>
    /// Whether the background job is enabled
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Interval in days between processing runs (default: 7)
    /// </summary>
    public int IntervalDays { get; set; } = 7;

    /// <summary>
    /// Gets the interval as TimeSpan
    /// </summary>
    public TimeSpan Interval => TimeSpan.FromDays(IntervalDays);

    /// <summary>
    /// Whether to run immediately on startup
    /// </summary>
    public bool RunOnStartup { get; set; } = true;

    /// <summary>
    /// Checks if any folder paths are configured
    /// </summary>
    public bool HasFolderPaths => FolderPaths.Count > 0 && FolderPaths.Any(p => !string.IsNullOrWhiteSpace(p));
}
