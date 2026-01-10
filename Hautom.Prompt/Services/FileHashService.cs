using System.Security.Cryptography;

namespace Hautom.Prompt.Services;

/// <summary>
/// Service for computing SHA256 file hashes
/// </summary>
public sealed class FileHashService : IFileHashService
{
    public string ComputeHash(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        var hashBytes = SHA256.HashData(stream);
        return Convert.ToHexString(hashBytes);
    }
}
