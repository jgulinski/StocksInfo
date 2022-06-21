using System.Text.Json.Serialization;

namespace Shared.Models;

public class PriceChangeDto
{
    [JsonPropertyName("symbol")] public string TickerSymbol { get; set; }
    [JsonPropertyName("1D")] public double Day { get; set; }
    [JsonPropertyName("5D")] public double FiveDays { get; set; }
    [JsonPropertyName("1M")] public double Month { get; set; }
}