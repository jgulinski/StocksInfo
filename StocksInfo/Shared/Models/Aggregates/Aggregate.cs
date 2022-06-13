namespace Shared.Models.Aggregates;

public class Aggregate
{
    public string TickerSymbol { get; set; }
    public DateTime Date { get; set; }
    public double Open { get; set; }
    public double Close { get; set; }
    public double High { get; set; }
    public double Low { get; set; }
    public int NumberOfTransactions { get; set; }
    // public String Volume { get; set; }
    public double AveragePrice { get; set; }
    
    public virtual Stock IdStockNavigation { get; set; }
}