using Shared.Models;

namespace Client.Services;

public interface IStockService
{
    Task<Stock?> GetStockInfo(string ticker);
    Task<List<FoundStockDto>?> SearchStocks(string searchText);
}