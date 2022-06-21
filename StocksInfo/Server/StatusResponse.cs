using System.Net;
using Shared.Models;
using Shared.Models.Aggregates;

namespace Server;

public class StatusResponse
{
    public HttpStatusCode StatusCode { get; set; }
    public string Message { get; set; }
    public UserDto UserDto { get; set; }

    public StockDto StockDto { get; set; }
    
    public List<FoundStockDto> FoundStockDtos { get; set; }

    public List<WatchlistDto> UserWatchlist { get; set; }

    public List<AggregateDto> Aggregates { get; set; }
    
    public List<PriceChangeDto> PriceChanges { get; set; }
    
}