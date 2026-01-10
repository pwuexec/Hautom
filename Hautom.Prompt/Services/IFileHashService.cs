namespace Hautom.Prompt.Services;

/// <summary>
/// Service for computing file hashes
/// </summary>
public interface IFileHashService
{
    /// <summary>
    /// Computes SHA256 hash of a file
    /// </summary>
    string ComputeHash(string filePath);
}
