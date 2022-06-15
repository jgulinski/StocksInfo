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

    public async Task<StockDto?> GetStockInfoAsync(string ticker)
    {
        var response = await _httpClient.GetAsync($"api/stocks/{ticker}");
            
        if (response.IsSuccessStatusCode)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                return default;
            }

            return await response.Content.ReadFromJsonAsync<StockDto>();
        }

        var message = await response.Content.ReadAsStringAsync();
        throw new Exception(message);
    }

    public async Task<List<FoundStockDto>> SearchStocksAsync(string searchText)
    {
        var response = await _httpClient.GetAsync($"api/stocks/search/{searchText}");
            
        if (response.IsSuccessStatusCode)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                return new List<FoundStockDto>();
            }

            return await response.Content.ReadFromJsonAsync<List<FoundStockDto>>();
        }

        var message = await response.Content.ReadAsStringAsync();
        throw new Exception(message);
    }

    public async Task<List<AggregateDto>> GetStockAggregatesAsync(string ticker)
    {
        var response = await _httpClient.GetAsync($"api/stocks/{ticker}/aggregates");
            
        if (response.IsSuccessStatusCode)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                return new List<AggregateDto>();
            }

            return await response.Content.ReadFromJsonAsync<List<AggregateDto>>();
        }

        var message = await response.Content.ReadAsStringAsync();
        throw new Exception(message);
    }
    
}