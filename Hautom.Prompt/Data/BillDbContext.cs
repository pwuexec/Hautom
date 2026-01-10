using Hautom.Prompt.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hautom.Prompt.Data;

/// <summary>
/// Database context for electricity bills
/// </summary>
public sealed class BillDbContext(DbContextOptions<BillDbContext> options) : DbContext(options)
{
    public DbSet<BillEntity> Bills { get; init; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BillEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.FileHash).IsUnique();
            entity.HasIndex(e => new { e.Year, e.Month });

            entity.Property(e => e.FilePath).IsRequired().HasMaxLength(500);
            entity.Property(e => e.FileHash).IsRequired().HasMaxLength(64);
            entity.Property(e => e.Month).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Period).IsRequired().HasMaxLength(100);

            entity.Property(e => e.BasePrice).HasPrecision(18, 6);
            entity.Property(e => e.DiscountValue).HasPrecision(18, 6);
            entity.Property(e => e.PriceAfterDiscount).HasPrecision(18, 6);
            entity.Property(e => e.ElectricityValue).HasPrecision(18, 2);
            entity.Property(e => e.TaxesAndFees).HasPrecision(18, 2);
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2);

            entity.Property(e => e.ProcessedAt).IsRequired();
            entity.Property(e => e.JsonData).IsRequired();
        });
    }
}
