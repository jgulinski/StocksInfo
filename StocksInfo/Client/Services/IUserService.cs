using Shared.Models;

namespace Client.Services;

public interface IUserService
{
    Task<List<WatchlistDto>> GetWatchlistAsync(string username);
    Task RemoveFromWatchlistAsync(string username, string ticker);
    Task AddToWatchlist(string username, string ticker);
}