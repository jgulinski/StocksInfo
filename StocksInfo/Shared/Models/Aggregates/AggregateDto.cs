namespace Shared.Models.Aggregates;

public class AggregateDto
{
    public string TickerSymbol { get; set; }
    public DateTime Date { get; set; }
    public double Open { get; set; }
    public double Close { get; set; }
    public double High { get; set; }
    public double Low { get; set; }
    public int NumberOfTransactions { get; set; }
    public decimal Volume { get; set; }
    public double AveragePrice { get; set; }
}