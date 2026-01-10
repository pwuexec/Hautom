namespace Hautom.Api.Filters;

/// <summary>
/// Filter for searching and paginating electricity bills
/// </summary>
public sealed record BillSearchFilter
{
    /// <summary>
    /// Filter by year (optional)
    /// </summary>
    public int? Year { get; init; }

    /// <summary>
    /// Filter by month name (optional, for future use)
    /// </summary>
    public string? Month { get; init; }

    /// <summary>
    /// Filter to show only offered months (optional, for future use)
    /// </summary>
    public bool? IsOfferedMonth { get; init; }

    /// <summary>
    /// Page number for pagination (1-based, default: 1)
    /// </summary>
    public int? Page { get; init; }

    /// <summary>
    /// Number of items per page (default: 20, max: 100)
    /// </summary>
    public int? PageSize { get; init; }

    /// <summary>
    /// Validates and normalizes the filter values
    /// </summary>
    public BillSearchFilter Normalize() => this with
    {
        Page = Math.Max(1, Page ?? 1),
        PageSize = Math.Clamp(PageSize ?? 20, 1, 100)
    };

    /// <summary>
    /// Gets the normalized page number
    /// </summary>
    public int GetPage() => Math.Max(1, Page ?? 1);

    /// <summary>
    /// Gets the normalized page size
    /// </summary>
    public int GetPageSize() => Math.Clamp(PageSize ?? 20, 1, 100);
}
