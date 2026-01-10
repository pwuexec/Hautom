# Hautom - Electricity Bill Extractor

Automated system for extracting data from electricity bill PDFs using modern .NET 10 features and functional error handling.

## Project Structure

```
Hautom.Prompt/
├── Configuration/
│   └── AppConfiguration.cs              # Application settings with Result validation
├── Data/
│   ├── Entities/
│   │   └── BillEntity.cs                # Database entity model
│   ├── Repositories/
│   │   ├── IBillRepository.cs           # Repository interface
│   │   └── BillRepository.cs            # Repository implementation
│   └── BillDbContext.cs                 # EF Core DbContext
├── Models/
│   ├── ElectricityBill.cs              # Main bill model
│   ├── ConsumptionDetails.cs           # Energy consumption data
│   ├── FinancialSummary.cs             # Financial information
│   └── Errors/
│       ├── BillExtractionError.cs      # Domain-specific errors
│       ├── ValidationError.cs          # Configuration validation errors
│       └── FileNotFoundError.cs        # File access errors
├── Services/
│   ├── IBillExtractorService.cs        # Extractor interface
│   ├── BillExtractorService.cs         # Extractor implementation
│   ├── IFileHashService.cs             # Hash computation interface
│   ├── FileHashService.cs              # SHA256 hash service
│   ├── IDatabaseInitializer.cs         # Database setup interface
│   ├── DatabaseInitializer.cs          # Database initialization
│   ├── JsonExportService.cs            # JSON export service
│   └── ConsoleLogger.cs                # Colored console logger
└── Program.cs                           # Application entry point
```

## .NET 10 Features Used

### 1. **Primary Constructors**
```csharp
public sealed class ConsoleLogger(bool verbose = false)
{
    // Constructor parameters available as fields throughout the class
    public void Debug(string message)
    {
        if (!verbose) return;
        // ...
    }
}
```

### 2. **Required Members & Init-only Properties**
```csharp
public sealed class ElectricityBill
{
    public required string Month { get; init; }
    public required int Year { get; init; }
    // Must be initialized when creating an instance
}
```

### 3. **Collection Expressions**
```csharp
private static readonly string[] OfferedMonthKeywords =
[
    "Tarifa Aniversário",
    "Fatura Aniversário"
];
```

### 4. **Source-Generated Regex** (.NET 10 Performance)
```csharp
public sealed partial class BillExtractorService
{
    [GeneratedRegex(@"(\d{2} \w{3} \d{4} a \d{2} \w{3} \d{4})")]
    private static partial Regex PeriodPattern();

    // Compiled at build time for maximum performance
}
```

### 5. **Expression-bodied Members**
```csharp
public string SerializeBill(ElectricityBill bill) =>
    JsonSerializer.Serialize(bill, DefaultOptions);

public decimal GetTaxPercentage() =>
    TotalAmount == 0 ? 0 : (TaxesAndFees / TotalAmount) * 100;
```

## FluentResults Pattern

**No more exceptions for flow control!** The application uses the **Result pattern** from FluentResults for elegant error handling.

### Before (Exception-based):
```csharp
try
{
    var bill = extractor.Extract(path);
    Process(bill);
}
catch (FileNotFoundException ex)
{
    Console.WriteLine($"File not found: {ex.Message}");
}
catch (BillExtractionException ex)
{
    Console.WriteLine($"Extraction failed: {ex.Message}");
}
```

### After (Result-based):
```csharp
var result = extractor.ExtractBillData(path);

if (result.IsSuccess)
{
    Process(result.Value);
}
else
{
    logger.Error($"Error: {result.Errors[0].Message}");
}
```

### Benefits:
- **Explicit error handling**: Errors are part of the type signature
- **No hidden control flow**: No exceptions thrown unexpectedly
- **Composable**: Results can be chained and transformed
- **Rich error context**: Metadata and causation tracking built-in
- **Better performance**: No stack unwinding overhead

## Key Improvements

### 1. Functional Error Handling
- **FluentResults** instead of exceptions
- Explicit `Result<T>` and `Result` return types
- Rich error metadata with context
- No try-catch blocks needed for business logic

### 2. .NET 10 Modern Features
- Primary constructors for concise syntax
- Required members for guaranteed initialization
- Init-only properties for immutability
- Collection expressions for cleaner syntax
- Source-generated regex for better performance

### 3. Domain-Driven Design
- Sealed classes prevent inheritance issues
- Explicit error types (`BillExtractionError`, `ValidationError`, etc.)
- Immutable models with `init` properties
- Clear separation of concerns

### 4. Clean Architecture
- Interface-based services for testability
- Dependency injection ready
- Single responsibility principle
- No static state or methods in business logic

### 5. Improved Type Safety
- Non-nullable reference types throughout
- Required properties enforce completeness
- Explicit Result types for all operations
- No null returns or exceptions for control flow

### 6. SQLite Database Persistence
- **Automatic deduplication**: SHA256 hash prevents reprocessing
- **Repository pattern**: Clean data access with Result types
- **Entity Framework Core 10**: Modern ORM with migrations support
- **Full bill history**: Complete JSON data stored for each bill
- **Performance**: Skip already processed files instantly

## Database Features

The application now includes **SQLite persistence** to avoid reprocessing bills:

### How It Works

1. **File Hash Computation**: SHA256 hash calculated for each PDF
2. **Duplicate Detection**: Database checked before extraction
3. **Skip Processed Files**: Already processed bills are skipped instantly
4. **Save After Extraction**: New bills saved to database with full JSON data

### Benefits

- **Faster Execution**: Avoid expensive PDF parsing for existing bills
- **Reliable Deduplication**: Content-based (hash) detection works even if filename changes
- **Audit Trail**: Complete history with timestamps and original data
- **Queryable Data**: Easy to generate reports and statistics

### Database Schema

Each bill record includes:
- File metadata (name, hash, processed date)
- Period information (month, year, period string)
- Consumption data (kWh, prices, discounts)
- Financial summary (electricity, taxes, total)
- Full JSON for complete data preservation

See [DATABASE.md](DATABASE.md) for detailed documentation.

## Usage

### Configuration
Edit the folder path in `AppConfiguration.CreateDefault()`:

```csharp
var config = new AppConfiguration
{
    FolderPath = @"C:\path\to\your\bills",
    FilePattern = "*.pdf"
};
```

### Running
```bash
cd Hautom.Prompt
dotnet run
```

### Output Example

**First run (processing new bills):**
```
[INFO] Database contains 0 processed bill(s)
[INFO] Processing bills from: C:\Users\pwuexec\Desktop\faturas\luz_2025
------------------------------------------------------------
[INFO] Found 5 file(s) to check
------------------------------------------------------------
[INFO] Processing: bill_january.pdf
[OK] Extracted: January/2025 - 150 kWh - €45.30
  >> Offered month detected!
[OK] Saved to database
{
  "DocumentType": "Electricity Bill",
  "Month": "January",
  "Year": 2025,
  "Period": "01 Jan 2025 a 31 Jan 2025",
  "IsOfferedMonth": true,
  "Consumption": { ... },
  "Financial": { ... }
}
------------------------------------------------------------
[INFO] Processing: bill_february.pdf
[OK] Extracted: February/2025 - 142 kWh - €41.80
[OK] Saved to database
------------------------------------------------------------
[INFO] Processing completed!
[OK] Bills processed and saved: 5
[INFO] Total bills in database: 5
```

**Second run (skipping already processed):**
```
[INFO] Database contains 5 processed bill(s)
[INFO] Processing bills from: C:\Users\pwuexec\Desktop\faturas\luz_2025
------------------------------------------------------------
[INFO] Found 7 file(s) to check
------------------------------------------------------------
[INFO] Skipping bill_january.pdf (already processed)
[INFO] Skipping bill_february.pdf (already processed)
[INFO] Skipping bill_march.pdf (already processed)
[INFO] Skipping bill_april.pdf (already processed)
[INFO] Skipping bill_may.pdf (already processed)
[INFO] Processing: bill_june.pdf
[OK] Extracted: June/2025 - 165 kWh - €48.20
[OK] Saved to database
------------------------------------------------------------
[INFO] Processing: bill_july.pdf
[OK] Extracted: July/2025 - 178 kWh - €52.15
[OK] Saved to database
------------------------------------------------------------
[INFO] Processing completed!
[OK] Bills processed and saved: 2
[INFO] Bills skipped (already in database): 5
[INFO] Total bills in database: 7
```

## Extracted Data

For each bill, the system extracts:

### Period and Date
- Complete period (e.g., "01 Jan 2025 a 31 Jan 2025")
- Month and Year separated

### Energy Consumption
- Total kWh consumed
- Base price per kWh
- Discount value
- Price after discount

### Financial Summary
- Electricity value (consumption)
- Taxes and fees value
- Total bill amount

### Offered Month Detection
- Automatically identifies bills with "Tarifa Aniversário"
- Detects bills with zero value but consumption

## Dependencies

- **.NET 10.0** - Latest .NET features
- **UglyToad.PdfPig** - PDF reading library
- **FluentResults 4.0** - Result pattern implementation
- **Entity Framework Core 10.0** - Modern ORM
- **Microsoft.EntityFrameworkCore.Sqlite 10.0** - SQLite database provider

## Technical Highlights

### Performance
- Source-generated regex (compile-time optimization)
- Minimal allocations with `init` properties
- Expression-bodied members reduce IL size

### Maintainability
- All code in English
- Consistent naming conventions
- XML documentation comments
- No exceptions for business logic

### Testability
- Interface-based abstractions
- Result pattern enables easy test assertions
- No side effects in core logic
- Dependency injection ready

## Future Extensions

1. Multi-format bill support (water, gas, etc.)
2. Export to CSV, Excel
3. REST API or GUI
4. Database storage
5. Consumption analytics and charts
6. JSON/CLI configuration
7. Unit and integration tests
8. Plugin system for different bill formats

## Best Practices Applied

- **SOLID principles**
- **Result pattern** (no exceptions for control flow)
- **Immutability** (init-only properties)
- **Domain-driven design** (rich error types)
- **Clean architecture** (separation of concerns)
- **Modern C# idioms** (.NET 10 features)
- **Type safety** (non-nullable, required members)
