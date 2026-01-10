using Hautom.Api.Configuration;
using Hautom.Prompt.Services;
using Microsoft.Extensions.Options;

namespace Hautom.Api.Services;

/// <summary>
/// Background service that periodically processes bill PDFs from a configured folder
/// </summary>
public sealed class BillProcessingBackgroundService(
    IServiceScopeFactory scopeFactory,
    IOptions<BillProcessingOptions> options,
    ILogger<BillProcessingBackgroundService> logger) : BackgroundService
{
    private readonly BillProcessingOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            logger.LogInformation("Bill processing background job is disabled");
            return;
        }

        if (!_options.HasFolderPaths)
        {
            logger.LogWarning("Bill processing folder paths are not configured. Background job will not run");
            return;
        }

        logger.LogInformation(
            "Bill processing background job started. Folders: {FolderPaths}, Interval: {Interval}",
            string.Join(", ", _options.FolderPaths),
            _options.Interval);

        if (_options.RunOnStartup)
        {
            await ProcessBillsAsync(stoppingToken);
        }

        using var timer = new PeriodicTimer(_options.Interval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await timer.WaitForNextTickAsync(stoppingToken);
                await ProcessBillsAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Expected when stopping
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in bill processing background job");
            }
        }

        logger.LogInformation("Bill processing background job stopped");
    }

    private async Task ProcessBillsAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting bill processing run at {Time}", DateTimeOffset.Now);

        try
        {
            using var scope = scopeFactory.CreateScope();
            var processingService = scope.ServiceProvider.GetRequiredService<IBillProcessingService>();

            var totalFilesFound = 0;
            var totalProcessed = 0;
            var totalSkipped = 0;
            var totalErrors = 0;
            var allErrorMessages = new List<string>();

            foreach (var folderPath in _options.FolderPaths.Where(p => !string.IsNullOrWhiteSpace(p)))
            {
                logger.LogInformation("Processing folder: {FolderPath}", folderPath);

                var result = await Task.Run(
                    () => processingService.ProcessFolder(folderPath, _options.FilePattern),
                    cancellationToken);

                if (result.IsSuccess)
                {
                    var stats = result.Value;
                    totalFilesFound += stats.FilesFound;
                    totalProcessed += stats.Processed;
                    totalSkipped += stats.Skipped;
                    totalErrors += stats.Errors;
                    allErrorMessages.AddRange(stats.ErrorMessages);
                }
                else
                {
                    logger.LogError("Bill processing failed for folder {Folder}: {Error}", folderPath, result.Errors[0].Message);
                    allErrorMessages.Add($"{folderPath}: {result.Errors[0].Message}");
                }
            }

            logger.LogInformation(
                "Bill processing completed. Found: {Found}, Processed: {Processed}, Skipped: {Skipped}, Errors: {Errors}",
                totalFilesFound,
                totalProcessed,
                totalSkipped,
                totalErrors);

            if (totalErrors > 0)
            {
                foreach (var error in allErrorMessages)
                {
                    logger.LogWarning("Processing error: {Error}", error);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error during bill processing");
        }
    }

    /// <summary>
    /// Manually trigger a processing run
    /// </summary>
    public async Task<Prompt.Services.ProcessingResult?> TriggerProcessingAsync(CancellationToken cancellationToken = default)
    {
        if (!_options.HasFolderPaths)
        {
            logger.LogWarning("Cannot trigger processing: folder paths are not configured");
            return null;
        }

        logger.LogInformation("Manual bill processing triggered");

        using var scope = scopeFactory.CreateScope();
        var processingService = scope.ServiceProvider.GetRequiredService<IBillProcessingService>();

        var totalFilesFound = 0;
        var totalProcessed = 0;
        var totalSkipped = 0;
        var totalErrors = 0;
        var allErrorMessages = new List<string>();

        foreach (var folderPath in _options.FolderPaths.Where(p => !string.IsNullOrWhiteSpace(p)))
        {
            var result = await Task.Run(
                () => processingService.ProcessFolder(folderPath, _options.FilePattern),
                cancellationToken);

            if (result.IsSuccess)
            {
                var stats = result.Value;
                totalFilesFound += stats.FilesFound;
                totalProcessed += stats.Processed;
                totalSkipped += stats.Skipped;
                totalErrors += stats.Errors;
                allErrorMessages.AddRange(stats.ErrorMessages);
            }
            else
            {
                allErrorMessages.Add($"{folderPath}: {result.Errors[0].Message}");
            }
        }

        return new Prompt.Services.ProcessingResult
        {
            FilesFound = totalFilesFound,
            Processed = totalProcessed,
            Skipped = totalSkipped,
            Errors = totalErrors,
            ErrorMessages = allErrorMessages
        };
    }
}
