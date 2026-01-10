using FluentResults;
using Hautom.Prompt.Data.Entities;
using Hautom.Prompt.Models;

namespace Hautom.Prompt.Data.Repositories;

/// <summary>
/// Repository for managing electricity bill persistence
/// </summary>
public interface IBillRepository
{
    /// <summary>
    /// Checks if a bill with the given file hash already exists
    /// </summary>
    Result<bool> ExistsByHash(string fileHash);

    /// <summary>
    /// Saves a bill to the database
    /// </summary>
    Result SaveBill(ElectricityBill bill, string fileHash, string jsonData);

    /// <summary>
    /// Gets all bills for a specific year
    /// </summary>
    Result<IReadOnlyList<BillEntity>> GetBillsByYear(int year);

    /// <summary>
    /// Gets all bills
    /// </summary>
    Result<IReadOnlyList<BillEntity>> GetAllBills();

    /// <summary>
    /// Gets the total count of processed bills
    /// </summary>
    Result<int> GetTotalCount();
}
