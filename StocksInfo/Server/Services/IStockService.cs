using Shared.Models;

namespace Server.Services;

public interface IStockService
{ 
    Task<StatusResponse> GetStockAsync(string ticker);
    Task<StatusResponse> GetStocksSearchResultAsync(string searchText);
    Task<StatusResponse> GetAggregatesAsync(string ticker);

    Task<StatusResponse> GetPriceChangesAsync(string tickers);
}