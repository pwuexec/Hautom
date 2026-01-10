using Hautom.Prompt.Data;
using Hautom.Prompt.Data.Repositories;
using Hautom.Prompt.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Hautom.Prompt.Extensions;

/// <summary>
/// Extension methods for registering Hautom.Prompt services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Hautom.Prompt services to the dependency injection container
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="databasePath">Path to the SQLite database file</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddHautomServices(
        this IServiceCollection services,
        string databasePath)
    {
        services.AddDbContext<BillDbContext>(options =>
            options.UseSqlite($"Data Source={databasePath}"));

        services.AddScoped<IBillRepository, BillRepository>();
        services.AddScoped<IDatabaseInitializer, DatabaseInitializer>();
        services.AddScoped<IBillExtractorService, BillExtractorService>();
        services.AddScoped<IFileHashService, FileHashService>();
        services.AddScoped<IBillProcessingService, BillProcessingService>();
        services.AddSingleton<JsonExportService>();

        return services;
    }
}
