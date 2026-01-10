namespace Hautom.Prompt.Models;

/// <summary>
/// Represents the financial summary of an electricity bill
/// </summary>
public sealed class FinancialSummary
{
    public decimal ElectricityValue { get; init; }
    public decimal TaxesAndFees { get; init; }
    public decimal TotalAmount { get; init; }

    /// <summary>
    /// Calculates the percentage that taxes represent of the total
    /// </summary>
    public decimal GetTaxPercentage() =>
        TotalAmount == 0 ? 0 : (TaxesAndFees / TotalAmount) * 100;

    /// <summary>
    /// Checks if this is a zero-value bill (offered)
    /// </summary>
    public bool IsZeroBill() =>
        TotalAmount == 0.00m;
}
