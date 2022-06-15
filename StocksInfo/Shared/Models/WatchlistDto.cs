namespace Shared.Models;

public class WatchlistDto
{
    public string TickerSymbol { get; set; }
    public string Name { get; set; }
    public string? PrimaryExchange { get; set; }
    public string? ImgUrl { get; set; }
}