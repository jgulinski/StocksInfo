using System.Net.Http.Json;
using Shared.Models;
using Shared.Models.Aggregates;

namespace Client.Services;

public class StockService : IStockService
{
    private readonly HttpClient _httpClient;

    public StockService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Stock?> GetStockInfo(string ticker)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/stocks/{ticker}");
            
            if (response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                {
                    return default(Stock);
                }

                return await response.Content.ReadFromJsonAsync<Stock>();
            }
            else
            {
                var message = await response.Content.ReadAsStringAsync();
                throw new Exception(message);
            }
        }

        catch (Exception e)
        {
            throw;
        }
    }

    public async Task<List<FoundStockDto>> SearchStocks(string searchText)
    {
        return await _httpClient.GetFromJsonAsync<List<FoundStockDto>>($"api/stocks/search/{searchText}");
    }

    public async Task<List<Aggregate>> GetStockAggregates(string ticker)
    {
        return await _httpClient.GetFromJsonAsync<List<Aggregate>>($"api/stocks/{ticker}/aggregates");
    }
    
}