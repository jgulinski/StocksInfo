using Shared.Models;
using Shared.Models.Aggregates;

namespace Client.Services;

public interface IStockService
{
    Task<StockDto?> GetStockInfoAsync(string ticker);
    Task<List<FoundStockDto>> SearchStocksAsync(string searchText);
    Task<List<AggregateDto>> GetStockAggregatesAsync(string ticker);
    Task<List<PriceChangeDto>> GetStocksPriceChange(List<string> tickers);
}