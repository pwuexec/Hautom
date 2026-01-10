using FluentResults;
using Hautom.Prompt.Models;

namespace Hautom.Prompt.Services;

/// <summary>
/// Service for extracting bill data from PDF files
/// </summary>
public interface IBillExtractorService
{
    /// <summary>
    /// Extracts data from an electricity bill PDF file
    /// </summary>
    /// <param name="filePath">Path to the PDF file</param>
    /// <returns>Result containing the extracted bill or error</returns>
    Result<ElectricityBill> ExtractBillData(string filePath);

    /// <summary>
    /// Processes multiple PDF files at once
    /// </summary>
    /// <param name="filePaths">Collection of PDF file paths</param>
    /// <returns>Collection of successfully extracted bills</returns>
    IReadOnlyList<Result<ElectricityBill>> ProcessMultipleFiles(IEnumerable<string> filePaths);
}
