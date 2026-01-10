using FluentResults;
using Hautom.Prompt.Data.Repositories;

namespace Hautom.Prompt.Services;

/// <summary>
/// Service for batch processing electricity bill PDFs
/// </summary>
public sealed class BillProcessingService(
    IBillRepository repository,
    IBillExtractorService extractorService,
    IFileHashService hashService,
    JsonExportService exportService) : IBillProcessingService
{
    public Result<ProcessingResult> ProcessFolder(string folderPath, string filePattern = "*.pdf")
    {
        if (string.IsNullOrWhiteSpace(folderPath))
            return Result.Fail<ProcessingResult>("Folder path cannot be empty");

        if (!Directory.Exists(folderPath))
            return Result.Fail<ProcessingResult>($"Directory not found: {folderPath}");

        var files = Directory.GetFiles(folderPath, filePattern);

        if (files.Length == 0)
        {
            return Result.Ok(new ProcessingResult
            {
                FilesFound = 0,
                Processed = 0,
                Skipped = 0,
                Errors = 0,
                ErrorMessages = []
            });
        }

        var processed = 0;
        var skipped = 0;
        var errors = 0;
        var errorMessages = new List<string>();

        foreach (var file in files)
        {
            var fileName = Path.GetFileName(file);

            try
            {
                // Compute file hash
                var fileHash = hashService.ComputeHash(file);

                // Check if already processed
                var existsResult = repository.ExistsByHash(fileHash);
                if (existsResult.IsFailed)
                {
                    errors++;
                    errorMessages.Add($"{fileName}: Database error - {existsResult.Errors[0].Message}");
                    continue;
                }

                if (existsResult.Value)
                {
                    skipped++;
                    continue;
                }

                // Extract bill data
                var extractResult = extractorService.ExtractBillData(file);

                if (extractResult.IsFailed)
                {
                    errors++;
                    errorMessages.Add($"{fileName}: Extraction failed - {extractResult.Errors[0].Message}");
                    continue;
                }

                var bill = extractResult.Value;

                // Serialize to JSON
                var jsonData = exportService.SerializeBill(bill);

                // Save to database
                var saveResult = repository.SaveBill(bill, fileHash, jsonData);
                if (saveResult.IsFailed)
                {
                    errors++;
                    errorMessages.Add($"{fileName}: Save failed - {saveResult.Errors[0].Message}");
                    continue;
                }

                processed++;
            }
            catch (Exception ex)
            {
                errors++;
                errorMessages.Add($"{fileName}: Unexpected error - {ex.Message}");
            }
        }

        return Result.Ok(new ProcessingResult
        {
            FilesFound = files.Length,
            Processed = processed,
            Skipped = skipped,
            Errors = errors,
            ErrorMessages = errorMessages
        });
    }
}
