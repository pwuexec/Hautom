using FluentResults;
using Hautom.Prompt.Data.Entities;
using Hautom.Prompt.Models;
using Microsoft.EntityFrameworkCore;

namespace Hautom.Prompt.Data.Repositories;

/// <summary>
/// Repository implementation for bill persistence
/// </summary>
public sealed class BillRepository(BillDbContext context) : IBillRepository
{
    public Result<bool> ExistsByHash(string fileHash)
    {
        try
        {
            var exists = context.Bills.Any(b => b.FileHash == fileHash);
            return Result.Ok(exists);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to check bill existence by hash: {ex.Message}");
        }
    }

    public Result SaveBill(ElectricityBill bill, string fileHash, string jsonData)
    {
        try
        {
            var entity = new BillEntity
            {
                FilePath = bill.FilePath,
                FileHash = fileHash,
                Month = bill.Month,
                Year = bill.Year,
                Period = bill.Period,
                IsOfferedMonth = bill.IsOfferedMonth,
                TotalKwh = bill.Consumption.TotalKwh,
                BasePrice = bill.Consumption.BasePrice,
                DiscountValue = bill.Consumption.DiscountValue,
                PriceAfterDiscount = bill.Consumption.PriceAfterDiscount,
                ElectricityValue = bill.Financial.ElectricityValue,
                TaxesAndFees = bill.Financial.TaxesAndFees,
                TotalAmount = bill.Financial.TotalAmount,
                ProcessedAt = DateTime.UtcNow,
                JsonData = jsonData
            };

            context.Bills.Add(entity);
            context.SaveChanges();

            return Result.Ok();
        }
        catch (DbUpdateException ex)
        {
            return Result.Fail($"Failed to save bill to database: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Result.Fail($"Unexpected error saving bill: {ex.Message}");
        }
    }

    public Result<IReadOnlyList<BillEntity>> GetBillsByYear(int year)
    {
        try
        {
            var bills = context.Bills
                .Where(b => b.Year == year)
                .OrderBy(b => b.Month)
                .ToList();

            return Result.Ok<IReadOnlyList<BillEntity>>(bills);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to retrieve bills for year {year}: {ex.Message}");
        }
    }

    public Result<IReadOnlyList<BillEntity>> GetAllBills()
    {
        try
        {
            var bills = context.Bills
                .OrderByDescending(b => b.Year)
                .ThenBy(b => b.Month)
                .ToList();

            return Result.Ok<IReadOnlyList<BillEntity>>(bills);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to retrieve bills: {ex.Message}");
        }
    }

    public Result<int> GetTotalCount()
    {
        try
        {
            var count = context.Bills.Count();
            return Result.Ok(count);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to get bill count: {ex.Message}");
        }
    }
}
