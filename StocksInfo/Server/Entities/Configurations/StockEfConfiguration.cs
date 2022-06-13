using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Models;

namespace Server.Entities.Configurations;

public class StockEfConfiguration : IEntityTypeConfiguration<Stock>
{
    public void Configure(EntityTypeBuilder<Stock> builder)
    {
        builder.HasKey(e => e.TickerSymbol).HasName("Stock_pk");

        builder.Property(e => e.TickerSymbol).IsRequired().HasMaxLength(10);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(100);
        builder.Property(e => e.PrimaryExchange).HasMaxLength(50);
        builder.Property(e => e.IndustrialClassification).HasMaxLength(100);
        builder.Property(e => e.HomepageUrl).HasMaxLength(400);
        builder.Property(e => e.ImgUrl).HasMaxLength(400);
        builder.Property(e => e.ListDate).HasColumnType("Date");
        builder.ToTable("Stock");

        builder.HasMany(e => e.Aggregates)
            .WithOne(a => a.IdStockNavigation)
            .OnDelete(DeleteBehavior.Cascade);

    }
}