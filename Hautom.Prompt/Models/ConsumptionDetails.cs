namespace Hautom.Prompt.Models;

/// <summary>
/// Represents detailed electricity consumption information
/// </summary>
public sealed class ConsumptionDetails
{
    public int TotalKwh { get; init; }
    public decimal BasePrice { get; init; }
    public decimal DiscountValue { get; init; }
    public decimal PriceAfterDiscount { get; init; }

    /// <summary>
    /// Calculates the price after applying the discount
    /// </summary>
    public static decimal CalculatePriceAfterDiscount(decimal basePrice, decimal discountValue) =>
        basePrice - discountValue;

    /// <summary>
    /// Returns the total cost of consumption
    /// </summary>
    public decimal GetTotalCost() =>
        TotalKwh * PriceAfterDiscount;

    /// <summary>
    /// Returns the discount percentage applied
    /// </summary>
    public decimal GetDiscountPercentage() =>
        BasePrice == 0 ? 0 : (DiscountValue / BasePrice) * 100;
}
