using System.ComponentModel;
using System.Text.Json.Serialization;
using Microsoft.VisualBasic.CompilerServices;

namespace Shared.Models.Aggregates;

public class StockAggregateJsonModel
{
    public string ticker { get; set; }
    public List<Result> results { get; set; }
    public string status { get; set; }
    public int resultsCount { get; set; }
}
public class Result
{
    // public string v { get; set; }
    public double vw { get; set; }
    public double o { get; set; }
    public double c { get; set; }
    public double h { get; set; }
    public double l { get; set; }
    public long t { get; set; }
    public int n { get; set; }
}


