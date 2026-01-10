using System.Text.Json;
using FluentResults;
using Hautom.Prompt.Models;

namespace Hautom.Prompt.Services;

/// <summary>
/// Service for exporting bills to JSON format
/// </summary>
public sealed class JsonExportService
{
    private static readonly JsonSerializerOptions DefaultOptions = new()
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    /// <summary>
    /// Serializes a bill to JSON
    /// </summary>
    public string SerializeBill(ElectricityBill bill) =>
        JsonSerializer.Serialize(bill, DefaultOptions);

    /// <summary>
    /// Serializes multiple bills to JSON
    /// </summary>
    public string SerializeMultipleBills(IEnumerable<ElectricityBill> bills) =>
        JsonSerializer.Serialize(bills, DefaultOptions);

    /// <summary>
    /// Exports a bill to a JSON file
    /// </summary>
    public Result ExportToFile(ElectricityBill bill, string outputPath)
    {
        try
        {
            var json = SerializeBill(bill);
            File.WriteAllText(outputPath, json);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to export to file: {ex.Message}");
        }
    }

    /// <summary>
    /// Exports multiple bills to a JSON file
    /// </summary>
    public Result ExportMultipleToFile(IEnumerable<ElectricityBill> bills, string outputPath)
    {
        try
        {
            var json = SerializeMultipleBills(bills);
            File.WriteAllText(outputPath, json);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to export to file: {ex.Message}");
        }
    }
}
