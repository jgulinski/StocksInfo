namespace Shared.Models;

public class StocksSearchResultJsonModel
{
    public List<Result> results { get; set; }
    public string status { get; set; }
}
public class Result
{
    public string ticker { get; set; }
    public string name { get; set; }
    public string primary_exchange { get; set; }
}


