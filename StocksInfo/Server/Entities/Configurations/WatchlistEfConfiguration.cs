using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Models;

namespace Server.Entities.Configurations;

public class WatchlistEfConfiguration : IEntityTypeConfiguration<Watchlist>
{
    public void Configure(EntityTypeBuilder<Watchlist> builder)
    {
        builder.HasKey(e =>
            new {IdStock = e.IdUser, e.Ticker}).HasName("Watchlist_pk");
        
        builder.ToTable("Watchlist");
        
        builder.HasOne(e => e.IdStockNavigation)
            .WithMany(e => e.Watchlists)
            .HasForeignKey(e => e.Ticker)
            .HasConstraintName("Watchlist_Stock")
            .OnDelete(DeleteBehavior.ClientSetNull);
        
        builder.HasOne(e => e.IdUserNavigation)
            .WithMany(e => e.Watchlists)
            .HasForeignKey(e => e.IdUser)
            .HasConstraintName("Watchlist_User")
            .OnDelete(DeleteBehavior.ClientSetNull);
    }
}