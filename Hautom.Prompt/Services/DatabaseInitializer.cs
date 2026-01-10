using FluentResults;
using Hautom.Prompt.Data;
using Microsoft.EntityFrameworkCore;

namespace Hautom.Prompt.Services;

/// <summary>
/// Service for initializing and ensuring database exists
/// </summary>
public sealed class DatabaseInitializer(BillDbContext context) : IDatabaseInitializer
{
    public Result Initialize()
    {
        try
        {
            // Ensure database is created
            context.Database.EnsureCreated();
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to initialize database: {ex.Message}");
        }
    }
}
