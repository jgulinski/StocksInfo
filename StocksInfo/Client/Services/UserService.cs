using System.Net.Http.Json;
using Shared.Models;

namespace Client.Services;

public class UserService : IUserService
{
    
    private readonly HttpClient _httpClient;

    public UserService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    
    
    
    
    public async Task<List<WatchlistDto>> GetWatchlistAsync(string username)
    {
        var response = await _httpClient.GetAsync($"api/user/{username}/watchlist");

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<List<WatchlistDto>>();
        }

        var message = await response.Content.ReadAsStringAsync();
        throw new Exception(message);
    }

    public async Task RemoveFromWatchlistAsync(string username, string ticker)
    {
        var response = await _httpClient.DeleteAsync($"api/user/{username}/watchlist/{ticker}");

        if (!response.IsSuccessStatusCode)
        {
            var message = await response.Content.ReadAsStringAsync();
            throw new Exception(message);
        }
    }

    public async Task AddToWatchlist(string username, string ticker)
    {
        var postBody = new {Ticker = ticker};
        var response = await _httpClient.PostAsJsonAsync($"api/user/{username}/watchlist/{ticker}", postBody);

        if (!response.IsSuccessStatusCode)
        {
            var message = await response.Content.ReadAsStringAsync();
            throw new Exception(message);
        }
    }
}