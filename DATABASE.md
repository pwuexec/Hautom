# Database Implementation Guide

## Overview

The application now includes SQLite database persistence to avoid reprocessing bills that have already been extracted. This significantly improves performance when running the application multiple times.

## Architecture

### Database Components

```
Data/
├── Entities/
│   └── BillEntity.cs              # Database entity model
├── Repositories/
│   ├── IBillRepository.cs         # Repository interface
│   └── BillRepository.cs          # Repository implementation
└── BillDbContext.cs               # EF Core DbContext
```

### Key Features

1. **File Hash Verification**: SHA256 hash computed for each PDF to detect duplicates
2. **Automatic Deduplication**: Files already processed are skipped
3. **Full Bill History**: Complete JSON data stored for each bill
4. **Result Pattern**: All database operations return `Result<T>` for consistent error handling

## Database Schema

### BillEntity Table

| Column | Type | Description |
|--------|------|-------------|
| Id | INTEGER | Primary key (auto-increment) |
| FileName | VARCHAR(255) | Original filename (unique) |
| FileHash | VARCHAR(64) | SHA256 hash of file content |
| Month | VARCHAR(20) | Month name (e.g., "January") |
| Year | INTEGER | Year |
| Period | VARCHAR(100) | Full period string |
| IsOfferedMonth | BOOLEAN | Whether this is an offered/free month |
| TotalKwh | INTEGER | Total energy consumption |
| BasePrice | DECIMAL(18,6) | Base price per kWh |
| DiscountValue | DECIMAL(18,6) | Discount value per kWh |
| PriceAfterDiscount | DECIMAL(18,6) | Final price per kWh |
| ElectricityValue | DECIMAL(18,2) | Electricity cost |
| TaxesAndFees | DECIMAL(18,2) | Taxes and fees |
| TotalAmount | DECIMAL(18,2) | Total bill amount |
| ProcessedAt | DATETIME | When the bill was processed (UTC) |
| JsonData | TEXT | Complete bill data as JSON |

### Indexes

- **Unique index** on `FileName` - prevents duplicate filenames
- **Index** on `FileHash` - fast duplicate detection
- **Composite index** on `(Year, Month)` - efficient year/month queries

## Usage Flow

### 1. Initialization

```csharp
// Configure SQLite connection
var optionsBuilder = new DbContextOptionsBuilder<BillDbContext>();
optionsBuilder.UseSqlite($"Data Source={config.DatabasePath}");

// Create DbContext and initialize database
using var dbContext = new BillDbContext(optionsBuilder.Options);
var dbInitializer = new DatabaseInitializer(dbContext);
var initResult = dbInitializer.Initialize();
```

### 2. Deduplication Check

```csharp
// Compute file hash
var fileHash = hashService.ComputeHash(filePath);

// Check if already processed
var existsResult = repository.ExistsByHash(fileHash);
if (existsResult.IsSuccess && existsResult.Value)
{
    // Skip this file - already in database
    return;
}
```

### 3. Save After Processing

```csharp
// Extract bill data
var extractResult = extractorService.ExtractBillData(file);

if (extractResult.IsSuccess)
{
    var bill = extractResult.Value;
    var jsonData = exportService.SerializeBill(bill);

    // Save to database
    var saveResult = repository.SaveBill(bill, fileHash, jsonData);
}
```

## Repository Methods

### `ExistsByHash(string fileHash)`
Returns `Result<bool>` indicating if a bill with the given hash exists.

### `ExistsByFileName(string fileName)`
Returns `Result<bool>` indicating if a bill with the given filename exists.

### `SaveBill(ElectricityBill bill, string fileHash, string jsonData)`
Saves a bill to the database. Returns `Result` indicating success or failure.

### `GetBillsByYear(int year)`
Returns `Result<IReadOnlyList<BillEntity>>` with all bills for a specific year.

### `GetAllBills()`
Returns `Result<IReadOnlyList<BillEntity>>` with all bills ordered by date.

### `GetTotalCount()`
Returns `Result<int>` with the total count of bills in the database.

## Configuration

Add the database path to your `AppConfiguration`:

```csharp
public sealed class AppConfiguration
{
    public string DatabasePath { get; init; } = "bills.db";
    // ... other properties
}
```

The default database file is `bills.db` in the application directory.

## Example Output

```
[INFO] Database contains 12 processed bill(s)
[INFO] Processing bills from: C:\Users\...\faturas\luz_2025
------------------------------------------------------------
[INFO] Found 15 file(s) to check
------------------------------------------------------------
[INFO] Skipping fatura_janeiro.pdf (already processed)
[INFO] Skipping fatura_fevereiro.pdf (already processed)
[INFO] Processing: fatura_marco.pdf
[OK] Extracted: March/2025 - 145 kWh - €42.15
[OK] Saved to database
------------------------------------------------------------
[INFO] Processing completed!
[OK] Bills processed and saved: 3
[INFO] Bills skipped (already in database): 12
[INFO] Total bills in database: 15
```

## Benefits

### 1. **Performance**
- Avoid expensive PDF parsing for already processed files
- SHA256 hash computation is much faster than full extraction
- Indexed database queries are extremely fast

### 2. **Reliability**
- Content-based deduplication (hash) detects identical files even with different names
- Atomic transactions ensure data consistency
- Result pattern prevents exceptions during database operations

### 3. **Data Persistence**
- Complete bill history stored permanently
- JSON data preserved for later analysis or export
- Easy to query bills by year, month, or other criteria

### 4. **Audit Trail**
- `ProcessedAt` timestamp for each bill
- Original filename preserved
- File hash allows verification of source data

## Future Enhancements

1. **Statistics Dashboard**: Query database for consumption trends and analytics
2. **Export Functionality**: Generate reports from database data
3. **Migration System**: Use EF Core migrations for schema changes
4. **Soft Deletes**: Mark bills as deleted instead of removing them
5. **Backup/Restore**: Implement database backup functionality
6. **Multi-tenancy**: Support multiple bill sources in one database

## Technical Details

### Entity Framework Core 10.0

The application uses the latest EF Core with SQLite provider:
- `Microsoft.EntityFrameworkCore.Sqlite 10.0.1`
- `Microsoft.EntityFrameworkCore.Design 10.0.1`

### Transaction Management

The DbContext handles transactions automatically. For custom transaction control:

```csharp
using var transaction = dbContext.Database.BeginTransaction();
try
{
    // Multiple operations
    repository.SaveBill(bill1, hash1, json1);
    repository.SaveBill(bill2, hash2, json2);

    transaction.Commit();
}
catch
{
    transaction.Rollback();
    throw;
}
```

### Connection String

Default: `Data Source=bills.db`

For custom location:
```csharp
DatabasePath = @"C:\MyData\electricity_bills.db"
```

## Troubleshooting

### Database Locked Error
If you get a "database is locked" error:
- Ensure only one instance of the application is running
- Check that no other process has the `.db` file open
- SQLite doesn't support concurrent writes

### Schema Changes
If you modify `BillEntity`:
1. Delete the existing `bills.db` file
2. Run the application to recreate the database
3. Or implement EF Core migrations for production environments

### Large Database
SQLite performs well up to several GB:
- For 1000 bills: ~10 MB database size
- For 10000 bills: ~100 MB database size
- Query performance remains excellent with proper indexes
