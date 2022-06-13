using Shared.Models;
using Shared.Models.Aggregates;

namespace Client.Services;

public interface IStockService
{
    Task<Stock?> GetStockInfo(string ticker);
    Task<List<FoundStockDto>> SearchStocks(string searchText);
    Task<List<Aggregate>> GetStockAggregates(string ticker);
}