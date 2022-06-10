namespace Shared.Models;

public class StockJsonModel
{
    public Content results { get; set; }
    public string status { get; set; }
    public string? error { get; set; }
}

public class Branding
{
    public string? logo_url { get; set; }
    public string? icon_url { get; set; }
}

public class Content
{
    public string ticker { get; set; }
    public string name { get; set; }
    public string? primary_exchange { get; set; }
    public string? description { get; set; }
    public string? sic_description { get; set; }
    public string? homepage_url { get; set; }
    public string? list_date { get; set; }
    public Branding? branding { get; set; }
}