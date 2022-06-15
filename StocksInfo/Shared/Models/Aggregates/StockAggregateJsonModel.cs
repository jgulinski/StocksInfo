using System.ComponentModel;
using System.Text.Json.Serialization;
using Microsoft.VisualBasic.CompilerServices;

namespace Shared.Models.Aggregates;

public class StockAggregateJsonModel
{
    public List<Result> results { get; set; }
    public string Status { get; set; }
    public int resultsCount { get; set; }
}

public class Result
{
    [JsonPropertyName("v")] public decimal Volume { get; set; }
    [JsonPropertyName("vw")] public double AveragePrice { get; set; }
    [JsonPropertyName("o")] public double Open { get; set; }
    [JsonPropertyName("c")] public double Close { get; set; }
    [JsonPropertyName("h")] public double High { get; set; }
    [JsonPropertyName("l")] public double Low { get; set; }
    [JsonPropertyName("t")] public long Time { get; set; }
    [JsonPropertyName("n")] public int NumberOfTransactions { get; set; }
}