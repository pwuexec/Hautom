namespace Hautom.Prompt.Models;

/// <summary>
/// Represents an electricity bill with consumption and financial details
/// </summary>
public sealed class ElectricityBill
{
    public required string DocumentType { get; init; } = "Electricity Bill";
    public required string Month { get; init; }
    public required int Year { get; init; }
    public required string Period { get; init; }
    public required string FilePath { get; init; }
    public bool IsOfferedMonth { get; init; }

    public required ConsumptionDetails Consumption { get; init; }
    public required FinancialSummary Financial { get; init; }

    /// <summary>
    /// Validates if the bill has the minimum required data
    /// </summary>
    public bool IsValid() =>
        !string.IsNullOrWhiteSpace(Month)
        && Year > 2000
        && !string.IsNullOrWhiteSpace(Period);

    /// <summary>
    /// Returns a text summary of the bill
    /// </summary>
    public string GetSummary() =>
        $"{Month}/{Year} - {Consumption.TotalKwh} kWh - €{Financial.TotalAmount:F2}";
}
