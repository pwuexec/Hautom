namespace Hautom.Api.Dtos;

/// <summary>
/// Data transfer object for electricity bill API responses
/// </summary>
public sealed record BillDto
{
    public required int Id { get; init; }
    public required string FilePath { get; init; }
    public required string Month { get; init; }
    public required int Year { get; init; }
    public required string Period { get; init; }
    public required bool IsOfferedMonth { get; init; }

    // Consumption data
    public required int TotalKwh { get; init; }
    public required decimal BasePrice { get; init; }
    public required decimal DiscountValue { get; init; }
    public required decimal PriceAfterDiscount { get; init; }

    // Financial data
    public required decimal ElectricityValue { get; init; }
    public required decimal TaxesAndFees { get; init; }
    public required decimal TotalAmount { get; init; }

    // Metadata
    public required DateTime ProcessedAt { get; init; }
}
