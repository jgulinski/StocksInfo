namespace Server.Services;

public interface IWatchlistService
{
    Task<StatusResponse> GetWatchlistAsync(string username);
    Task<StatusResponse> AddToWatchlistAsync(string username, string ticker);
    Task<StatusResponse> DeleteFromWatchlistAsync(string username, string ticker);
}