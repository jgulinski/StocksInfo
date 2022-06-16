using System.Text.Json.Serialization;

namespace Shared.Models;

// json property name 
public class StockJsonModel
{
    public Content Results { get; set; }
    public string Status { get; set; }
    public string? Error { get; set; }
}

public class Branding
{
    [JsonPropertyName("logo_url")] public string? LogoUrl { get; set; }
    [JsonPropertyName("icon_url")] public string? IconUrl { get; set; }
}

public class Content
{
    public string Ticker { get; set; }
    public string Name { get; set; }

    [JsonPropertyName("primary_exchange")] public string? MarketIdentifier { get; set; }
    public string? Description { get; set; }
    [JsonPropertyName("sic_description")] public string? IndustrialClassification { get; set; }
    [JsonPropertyName("homepage_url")]public string? HomepageUrl { get; set; }
    [JsonPropertyName("list_date")] public string? ListDate { get; set; }
    public Branding? Branding { get; set; }
}