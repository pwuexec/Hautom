using FluentResults;

namespace Hautom.Prompt.Services;

/// <summary>
/// Service for initializing the database
/// </summary>
public interface IDatabaseInitializer
{
    /// <summary>
    /// Ensures the database is created and migrated
    /// </summary>
    Result Initialize();
}
