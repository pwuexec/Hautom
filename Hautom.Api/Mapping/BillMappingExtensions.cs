using Hautom.Api.Dtos;
using Hautom.Prompt.Data.Entities;

namespace Hautom.Api.Mapping;

/// <summary>
/// Extension methods for mapping bill entities to DTOs
/// </summary>
public static class BillMappingExtensions
{
    public static BillDto ToDto(this BillEntity entity) => new()
    {
        Id = entity.Id,
        FilePath = entity.FilePath,
        Month = entity.Month,
        Year = entity.Year,
        Period = entity.Period,
        IsOfferedMonth = entity.IsOfferedMonth,
        TotalKwh = entity.TotalKwh,
        BasePrice = entity.BasePrice,
        DiscountValue = entity.DiscountValue,
        PriceAfterDiscount = entity.PriceAfterDiscount,
        ElectricityValue = entity.ElectricityValue,
        TaxesAndFees = entity.TaxesAndFees,
        TotalAmount = entity.TotalAmount,
        ProcessedAt = entity.ProcessedAt
    };

    public static IReadOnlyList<BillDto> ToDtos(this IEnumerable<BillEntity> entities) =>
        entities.Select(e => e.ToDto()).ToList();
}
