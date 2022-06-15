using System.Text.Json.Serialization;

namespace Shared.Models;

public class StocksSearchResultJsonModel
{
    public List<Result> Results { get; set; }
    public string Status { get; set; }
}
public class Result
{
    public string Ticker { get; set; }
    public string Name { get; set; }
    [JsonPropertyName("primary_exchange")]public string PrimaryExchange { get; set; }
}


