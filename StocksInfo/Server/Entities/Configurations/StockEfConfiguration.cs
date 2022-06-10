using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Models;

namespace Server.Entities.Configurations;

public class StockEfConfiguration : IEntityTypeConfiguration<Stock>
{
    public void Configure(EntityTypeBuilder<Stock> builder)
    {
        builder.HasKey(e => e.IdStock).HasName("Stock_pk");
        builder.Property(e => e.IdStock).UseIdentityColumn();

        builder.Property(e => e.TickerSymbol).IsRequired().HasMaxLength(10);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(100);
        builder.Property(e => e.PrimaryExchange).HasMaxLength(50);
        builder.Property(e => e.IndustrialClassification).HasMaxLength(100);
        builder.Property(e => e.HomepageUrl).HasMaxLength(400);
        builder.ToTable("Stock");
        
    }
}