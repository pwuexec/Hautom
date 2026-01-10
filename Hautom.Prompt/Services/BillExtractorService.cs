using System.Text.RegularExpressions;
using FluentResults;
using Hautom.Prompt.Models;
using Hautom.Prompt.Models.Errors;
using UglyToad.PdfPig;

namespace Hautom.Prompt.Services;

/// <summary>
/// Service for extracting electricity bill data from PDF files
/// </summary>
public sealed partial class BillExtractorService : IBillExtractorService
{
    // Regex patterns organized as generated partial methods (.NET 10 source generators)
    [GeneratedRegex(@"(\d{2} \w{3} \d{4} a \d{2} \w{3} \d{4})")]
    private static partial Regex PeriodPattern();

    [GeneratedRegex(@"Termo de Energia \(Real\).*?(\d,\d{6})")]
    private static partial Regex BasePricePattern();

    [GeneratedRegex(@"Desconto Termo de Energia Social.*?(\d,\d{6})")]
    private static partial Regex DiscountPattern();

    [GeneratedRegex(@"Imposto Especial Consumo \(Real\)\s*(\d+)\s*kWh")]
    private static partial Regex TotalConsumptionPattern();

    [GeneratedRegex(@"TOTAL Luz \(Consumo\)\s*(\d+,\d{2})")]
    private static partial Regex ElectricityValuePattern();

    [GeneratedRegex(@"TOTAL Taxas e Impostos\s*(\d+,\d{2})")]
    private static partial Regex TaxesValuePattern();

    [GeneratedRegex(@"TOTAL DA FATURA DE LUZ\s*(\d+,\d{2})")]
    private static partial Regex TotalValuePattern();

    private static readonly string[] OfferedMonthKeywords =
    [
        "Tarifa Aniversário",
        "Fatura Aniversário"
    ];

    public Result<ElectricityBill> ExtractBillData(string filePath)
    {
        if (!File.Exists(filePath))
            return Result.Fail(new FileNotFoundError(filePath));

        try
        {
            using var document = PdfDocument.Open(filePath);

            // Read detail pages (2 and 3)
            var pages = document.GetPages().Skip(1).Take(2).ToList();
            if (pages.Count == 0)
                return Result.Fail(new BillExtractionError("PDF doesn't contain enough pages", filePath));

            var text = string.Join(" ", pages.Select(p => p.Text));

            var periodResult = ExtractPeriodAndDate(text);
            if (periodResult.IsFailed)
                return Result.Fail(new BillExtractionError("Failed to extract period", filePath)
                    .WithInnerException(new InvalidDataException(periodResult.Errors.First().Message)));

            var consumptionResult = ExtractConsumption(text);
            if (consumptionResult.IsFailed)
                return Result.Fail(new BillExtractionError("Failed to extract consumption", filePath)
                    .WithInnerException(new InvalidDataException(consumptionResult.Errors.First().Message)));

            var financialResult = ExtractFinancialSummary(text);
            if (financialResult.IsFailed)
                return Result.Fail(new BillExtractionError("Failed to extract financial data", filePath)
                    .WithInnerException(new InvalidDataException(financialResult.Errors.First().Message)));

            var (month, year, period) = periodResult.Value;
            var isOffered = DetermineOfferedMonth(text, financialResult.Value, consumptionResult.Value);

            var bill = new ElectricityBill
            {
                DocumentType = "Electricity Bill",
                Month = month,
                Year = year,
                Period = period,
                FilePath = filePath,
                IsOfferedMonth = isOffered,
                Consumption = consumptionResult.Value,
                Financial = financialResult.Value
            };

            return Result.Ok(bill);
        }
        catch (Exception ex)
        {
            return Result.Fail(new BillExtractionError(
                $"Error processing file: {Path.GetFileName(filePath)}", filePath)
                .WithInnerException(ex));
        }
    }

    public IReadOnlyList<Result<ElectricityBill>> ProcessMultipleFiles(IEnumerable<string> filePaths) =>
        filePaths.Select(ExtractBillData).ToList();

    private static Result<(string Month, int Year, string Period)> ExtractPeriodAndDate(string text)
    {
        var periodMatch = PeriodPattern().Match(text);
        if (!periodMatch.Success)
            return Result.Fail("Period pattern not found in document");

        var period = periodMatch.Value;
        var parts = period.Split(' ');

        if (parts.Length < 7)
            return Result.Fail("Invalid period format");

        var month = TranslateMonth(parts[5]);
        if (!int.TryParse(parts[6], out var year))
            return Result.Fail("Invalid year format");

        return Result.Ok((month, year, period));
    }

    private static Result<ConsumptionDetails> ExtractConsumption(string text)
    {
        var basePriceStr = BasePricePattern().Match(text).Groups[1].Value;
        var discountStr = DiscountPattern().Match(text).Groups[1].Value;
        var consumptionStr = TotalConsumptionPattern().Match(text).Groups[1].Value;

        var basePrice = ParseDecimal(basePriceStr);
        var discountValue = ParseDecimal(discountStr);
        var totalKwh = int.TryParse(consumptionStr, out var kwh) ? kwh : 0;

        var consumption = new ConsumptionDetails
        {
            TotalKwh = totalKwh,
            BasePrice = basePrice,
            DiscountValue = discountValue,
            PriceAfterDiscount = ConsumptionDetails.CalculatePriceAfterDiscount(basePrice, discountValue)
        };

        return Result.Ok(consumption);
    }

    private static Result<FinancialSummary> ExtractFinancialSummary(string text)
    {
        var electricityValue = ParseDecimal(ElectricityValuePattern().Match(text).Groups[1].Value);
        var taxesValue = ParseDecimal(TaxesValuePattern().Match(text).Groups[1].Value);
        var totalValue = ParseDecimal(TotalValuePattern().Match(text).Groups[1].Value);

        var financial = new FinancialSummary
        {
            ElectricityValue = electricityValue,
            TaxesAndFees = taxesValue,
            TotalAmount = totalValue
        };

        return Result.Ok(financial);
    }

    private static bool DetermineOfferedMonth(string text, FinancialSummary financial, ConsumptionDetails consumption)
    {
        var hasKeyword = OfferedMonthKeywords.Any(keyword =>
            text.Contains(keyword, StringComparison.OrdinalIgnoreCase));

        var isZeroTotal = financial.IsZeroBill() && consumption.TotalKwh > 0;

        return hasKeyword || isZeroTotal;
    }

    private static decimal ParseDecimal(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return 0.00m;

        decimal.TryParse(value.Replace(",", "."), out var result);
        return result;
    }

    private static string TranslateMonth(string abbreviation) => abbreviation.ToLower() switch
    {
        "jan" => "January",
        "fev" => "February",
        "mar" => "March",
        "abr" => "April",
        "mai" => "May",
        "jun" => "June",
        "jul" => "July",
        "ago" => "August",
        "set" => "September",
        "out" => "October",
        "nov" => "November",
        "dez" => "December",
        _ => abbreviation
    };
}
