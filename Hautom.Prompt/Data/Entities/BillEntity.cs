using System.ComponentModel.DataAnnotations;

namespace Hautom.Prompt.Data.Entities;

/// <summary>
/// Database entity representing a processed electricity bill
/// </summary>
public sealed class BillEntity
{
    [Key]
    public int Id { get; init; }

    public required string FilePath { get; init; }
    public required string FileHash { get; init; }
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
    public required string JsonData { get; init; }
}
