namespace Shared.Models;

public class Watchlist
{
    public int IdUser { get; set; }
    public string TickerSymbol { get; set; }
    public virtual User IdUserNavigation { get; set; }
    public virtual Stock IdStockNavigation { get; set; }
}