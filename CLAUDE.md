# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Hautom** is an electricity bill PDF extraction system built with .NET 10, featuring:
- **Hautom.Prompt**: Class library containing core extraction logic, data access, and domain models
- **Hautom.Api**: Minimal API providing read-only access to bill data

The project uses modern .NET 10 features (primary constructors, required members, collection expressions, source-generated regex) and the FluentResults pattern for functional error handling.

## Solution Structure

```
Hautom/
├── Hautom.Prompt/          # Class library - core extraction and data access
│   ├── Configuration/      # App settings with Result validation
│   ├── Data/              # EF Core entities, repositories, DbContext
│   ├── Extensions/        # DI registration (ServiceCollectionExtensions)
│   ├── Models/            # Domain models and error types
│   └── Services/          # Business logic (extraction, processing, hashing)
└── Hautom.Api/            # Minimal API with web dashboard
    ├── Configuration/     # BillProcessingOptions
    ├── Dtos/              # API response DTOs (BillDto, PaginatedResponse)
    ├── Filters/           # Query filters (BillSearchFilter)
    ├── Mapping/           # Entity-to-DTO mapping extensions
    ├── Services/          # BillProcessingBackgroundService
    ├── wwwroot/           # Static files (dashboard)
    │   └── index.html     # Dark mode dashboard with charts
    └── Program.cs         # API host with endpoints and background job
```

## Common Commands

```bash
# Build entire solution
dotnet build Hautom.sln

# Run API (dev mode exposes OpenAPI at /openapi/v1.json)
dotnet run --project Hautom.Api

# Entity Framework (from Hautom.Prompt directory)
dotnet ef migrations add MigrationName --startup-project ../Hautom.Api
dotnet ef database update --startup-project ../Hautom.Api

# Reset database (delete bills.db and run API to recreate)
```

## API Endpoints

```
GET  /bills                    - All bills (paginated)
GET  /bills?year=2025          - Bills for specific year
GET  /bills?page=2&pageSize=10 - Pagination control
POST /bills/process            - Manually trigger bill processing
```

The GET endpoint returns `PaginatedResponse<BillDto>` with items, totalCount, page, pageSize, and pagination metadata.

## Web Dashboard

Navigate to the root URL (`/`) to access the dark mode dashboard featuring:
- Summary cards (total spent, average cost, consumption, free months)
- Monthly cost trend line chart
- Energy consumption bar chart
- Cost breakdown pie chart (electricity vs taxes)
- Price per kWh comparison chart
- Bills table with "View Invoice" modal (PDF download mocked)
- Year filter dropdown
- "Process Bills" button to manually trigger processing

The dashboard uses Chart.js and fetches data from the `/bills` API endpoint.

## Background Processing

A background job automatically processes PDF bills from a configured folder:

- **Weekly schedule** (configurable via `IntervalDays`)
- **Run on startup** option
- **Deduplication** via SHA256 hash (skips already processed files)
- **Manual trigger** via dashboard button or `POST /bills/process`

Configure in `appsettings.json`:

```json
{
  "BillProcessing": {
    "FolderPath": "C:\\path\\to\\bills",
    "FilePattern": "*.pdf",
    "Enabled": true,
    "IntervalDays": 7,
    "RunOnStartup": true
  }
}
```

The `IBillProcessingService` handles batch processing and returns `ProcessingResult` with counts for files found, processed, skipped, and errors.

## Architecture Principles

### Result Pattern (FluentResults)

**Critical:** All operations that can fail return `Result<T>` or `Result`. Never use exceptions for business logic.

```csharp
public Result<ElectricityBill> ExtractBillData(string filePath)
{
    if (!File.Exists(filePath))
        return Result.Fail(new FileNotFoundError(filePath));
    return Result.Ok(bill);
}

// Usage
var result = service.ExtractBillData(path);
if (result.IsSuccess)
    Process(result.Value);
else
    logger.Error(result.Errors[0].Message);
```

- Custom error types in `Models/Errors/` inherit from `IError`
- Check `IsSuccess`/`IsFailed` before accessing `Value`

### Dependency Injection

Use `AddHautomServices()` extension to register all library services:

```csharp
builder.Services.AddHautomServices(databasePath);
```

This registers: `BillDbContext`, `IBillRepository`, `IDatabaseInitializer`, `IBillExtractorService`, `IFileHashService`, `IBillProcessingService`, `JsonExportService`

### .NET 10 Features

**Primary Constructors:**
```csharp
public sealed class BillDbContext(DbContextOptions<BillDbContext> options) : DbContext(options) { }
```

**Required Members + Init Properties:**
```csharp
public sealed class ElectricityBill
{
    public required string Month { get; init; }
}
```

**Source-Generated Regex:**
```csharp
public sealed partial class BillExtractorService
{
    [GeneratedRegex(@"(\d{2} \w{3} \d{4})")]
    private static partial Regex DatePattern();
}
```

**Collection Expressions:**
```csharp
private static readonly string[] Keywords = ["Tarifa", "Fatura"];
```

### Immutability

Models use `init` properties - immutable after construction. Always use object initializers.

### Database Deduplication

SHA256 file hashing prevents reprocessing:
1. `FileHashService` computes hash for each PDF
2. `BillRepository.ExistsByHash()` checks if processed
3. Skip if exists, otherwise extract and save with hash

### Service Interfaces

All services have interfaces (`IBillExtractorService`, `IFileHashService`, `IBillRepository`, `IDatabaseInitializer`). New services should:
1. Create interface in `Services/`
2. Implement as sealed class with primary constructor
3. Return `Result<T>` types
4. Register in `ServiceCollectionExtensions.AddHautomServices()`

## PDF Extraction Logic

`BillExtractorService` reads pages 2-3 of PDFs (detail pages) using source-generated regex patterns optimized for Portuguese electricity bills:

- `ExtractPeriodAndDate()` - Period string, Month/Year
- `ExtractConsumption()` - kWh, prices, discounts
- `ExtractFinancialSummary()` - Electricity value, taxes, total
- `DetermineOfferedMonth()` - Detects "Tarifa Aniversário" (free month)

## Configuration

API configuration in `appsettings.json`:

```json
{
  "Hautom": {
    "DatabasePath": "bills.db"
  }
}
```

For batch processing, create an `AppConfiguration` instance with `FolderPath` and `DatabasePath`.

## Database

**BillEntity** stores file metadata, period info, consumption, financials, and full JSON data. Key indexes:
- Unique on `FileName`
- Index on `FileHash` (duplicate detection)
- Composite on `(Year, Month)`

See `DATABASE.md` for details.

## Code Style

- **Sealed classes** - prevent inheritance
- **English only** - code and comments
- **Expression-bodied members** - for simple getters
- **Non-nullable types** - enabled project-wide
- **Records for DTOs** - use `record` types in API DTOs

## Dependencies

**Hautom.Prompt:**
- **UglyToad.PdfPig**: PDF parsing
- **FluentResults 4.0.0**: Result pattern
- **Microsoft.EntityFrameworkCore.Sqlite 10.0.1**: SQLite ORM

**Hautom.Api:**
- **Microsoft.AspNetCore.OpenApi 10.0.1**: API documentation
- References Hautom.Prompt for all business logic

Target: **net10.0**

## Current Limitations

- Single Portuguese electricity bill format
- No tests
- Database recreated on schema changes (no migrations)
- PDF viewing in dashboard is mocked (no actual file serving yet)
