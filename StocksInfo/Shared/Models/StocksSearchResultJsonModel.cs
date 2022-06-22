using System.Text.Json.Serialization;

namespace Shared.Models;

public class StocksSearchResultJsonModel
{
    [JsonPropertyName("symbol")] public string TickerSymbol { get; set; }
    public string Name { get; set; }

    [JsonPropertyName("exchangeShortName")]
    public string StockExchange { get; set; }
}