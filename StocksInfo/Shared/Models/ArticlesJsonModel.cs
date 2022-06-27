using System.Text.Json.Serialization;

namespace Shared.Models;

public class Publisher
{
    public string Name { get; set; }
}

public class Result
{
    public Publisher Publisher { get; set; }
    public string Title { get; set; }
    public string Author { get; set; }
    [JsonPropertyName("published_utc")] public DateTime Published { get; set; }
    [JsonPropertyName("article_url")] public string Url { get; set; }
    public string Description { get; set; }
}

public class ArticlesJsonModel
{
    public List<Result> Results { get; set; }
    public string Status { get; set; }
}

