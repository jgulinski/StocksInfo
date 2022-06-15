namespace Shared.Models;

public class StockDto
{
    public string TickerSymbol { get; set; }
    public string Name { get; set; }
    public string? PrimaryExchange { get; set; }
    public string? Description { get; set; }
    public string? IndustrialClassification { get; set; }
    public string? HomepageUrl { get; set; }
    public DateTime? ListDate { get; set; }
    public string? ImgUrl { get; set; }
}