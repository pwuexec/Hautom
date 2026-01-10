using Hautom.Api.Configuration;
using Hautom.Api.Dtos;
using Hautom.Api.Filters;
using Hautom.Api.Mapping;
using Hautom.Api.Services;
using Hautom.Prompt.Data.Repositories;
using Hautom.Prompt.Extensions;
using Hautom.Prompt.Services;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

// Configure Hautom services
var databasePath = builder.Configuration.GetValue<string>("Hautom:DatabasePath") ?? "bills.db";
builder.Services.AddHautomServices(databasePath);

// Configure bill processing background job
builder.Services.Configure<BillProcessingOptions>(
    builder.Configuration.GetSection(BillProcessingOptions.SectionName));
builder.Services.AddSingleton<BillProcessingBackgroundService>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<BillProcessingBackgroundService>());

var app = builder.Build();

// Initialize database on startup
using (var scope = app.Services.CreateScope())
{
    var dbInitializer = scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>();
    var initResult = dbInitializer.Initialize();
    if (initResult.IsFailed)
    {
        throw new InvalidOperationException($"Database initialization failed: {initResult.Errors[0].Message}");
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();

// GET /bills - Retrieve bills with optional filtering
app.MapGet("/bills", (
    IBillRepository repository,
    [AsParameters] BillSearchFilter filter) =>
{
    var result = filter.Year.HasValue
        ? repository.GetBillsByYear(filter.Year.Value)
        : repository.GetAllBills();

    if (result.IsFailed)
    {
        return Results.Problem(
            detail: result.Errors[0].Message,
            statusCode: StatusCodes.Status500InternalServerError);
    }

    var bills = result.Value;
    var totalCount = bills.Count;

    var page = filter.GetPage();
    var pageSize = filter.GetPageSize();

    var pagedBills = bills
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToDtos();

    var response = new PaginatedResponse<BillDto>
    {
        Items = pagedBills,
        TotalCount = totalCount,
        Page = page,
        PageSize = pageSize
    };

    return Results.Ok(response);
})
.WithName("GetBills")
.WithDescription("Retrieves electricity bills with optional year filter and pagination");

// POST /bills/process - Manually trigger bill processing
app.MapPost("/bills/process", async (
    BillProcessingBackgroundService backgroundService,
    CancellationToken cancellationToken) =>
{
    var result = await backgroundService.TriggerProcessingAsync(cancellationToken);

    if (result is null)
    {
        return Results.Problem(
            detail: "Processing failed or folder path not configured",
            statusCode: StatusCodes.Status500InternalServerError);
    }

    return Results.Ok(new
    {
        result.FilesFound,
        result.Processed,
        result.Skipped,
        result.Errors,
        result.ErrorMessages
    });
})
.WithName("ProcessBills")
.WithDescription("Manually trigger bill processing from configured folder");

// GET /bills/pdf/{id} - Serve PDF file for preview by bill ID
app.MapGet("/bills/pdf/{id:int}", (
    int id,
    IBillRepository repository,
    IOptions<BillProcessingOptions> options) =>
{
    var billResult = repository.GetAllBills();
    if (billResult.IsFailed)
    {
        return Results.Problem(
            detail: billResult.Errors[0].Message,
            statusCode: StatusCodes.Status500InternalServerError);
    }

    var bill = billResult.Value.FirstOrDefault(b => b.Id == id);
    if (bill is null)
    {
        return Results.NotFound($"Bill not found: {id}");
    }

    var filePath = bill.FilePath;

    // Validate the file path is within configured folders (security check)
    var normalizedPath = Path.GetFullPath(filePath);
    var isInConfiguredFolder = options.Value.FolderPaths
        .Where(p => !string.IsNullOrWhiteSpace(p))
        .Any(folder => normalizedPath.StartsWith(Path.GetFullPath(folder), StringComparison.OrdinalIgnoreCase));

    if (!isInConfiguredFolder)
    {
        return Results.Problem(
            detail: "File path is outside configured folders",
            statusCode: StatusCodes.Status403Forbidden);
    }

    if (!File.Exists(filePath))
    {
        return Results.NotFound($"File not found at path: {filePath}");
    }

    var fileStream = File.OpenRead(filePath);
    return Results.File(fileStream, "application/pdf", Path.GetFileName(filePath));
})
.WithName("GetBillPdf")
.WithDescription("Retrieve PDF file for preview by bill ID");

app.Run();
