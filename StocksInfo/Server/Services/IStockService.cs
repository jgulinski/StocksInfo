using Shared.Models;

namespace Server.Services;

public interface IStockService
{ 
    Task<Stock?> GetStockAsync(string ticker);
    Task<List<FoundStockDto>> GetStocksSearchResultAsync(string searchText);
}