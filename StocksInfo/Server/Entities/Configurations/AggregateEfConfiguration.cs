using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Models.Aggregates;

namespace Server.Entities.Configurations;

public class AggregateEfConfiguration : IEntityTypeConfiguration<Aggregate>
{
    public void Configure(EntityTypeBuilder<Aggregate> builder)
    {
        builder.HasKey(e =>
            new {IdStock = e.TickerSymbol, e.Date}).HasName("Aggregate_pk");


        builder.Property(e => e.Date).IsRequired().HasColumnType("Date");

        builder.HasOne(e => e.IdStockNavigation)
            .WithMany(e => e.Aggregates)
            .HasForeignKey(e => e.TickerSymbol)
            .HasConstraintName("Aggregate_Stock")
            .OnDelete(DeleteBehavior.ClientSetNull);
        
        
        builder.ToTable("Aggregate");
    }
}