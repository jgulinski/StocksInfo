using Shared.Models.Aggregates;

namespace Shared.Models;

public class Stock
{
    public string TickerSymbol { get; set; }
    public string Name { get; set; }
    public string? MarketIdentifier { get; set; }
    public string? Description { get; set; }
    public string? IndustrialClassification { get; set; }
    public string? HomepageUrl { get; set; }
    public DateTime? ListDate { get; set; }
    public string? ImgUrl { get; set; }

    public virtual ICollection<Aggregate> Aggregates { get; set; }

    public virtual ICollection<Watchlist> Watchlists { get; set; }

    public Stock()
    {
        Aggregates = new HashSet<Aggregate>();
        Watchlists = new HashSet<Watchlist>();
    }
}