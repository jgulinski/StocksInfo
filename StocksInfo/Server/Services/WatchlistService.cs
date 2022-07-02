using System.Net;
using Microsoft.EntityFrameworkCore;
using Server.Entities;
using Shared.Models;

namespace Server.Services;

public class WatchlistService : IWatchlistService
{
    private readonly ApiContext _context;
    private readonly IConfiguration _configuration;


    public WatchlistService(ApiContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<StatusResponse> GetWatchlistAsync(string username)
    {
        var stocks =
            await _context.Watchlists.Where(e => e.IdUserNavigation.EmailAddress == username)
                .Select(e => e.IdStockNavigation).ToListAsync();
        var watchlistToReturn = new List<WatchlistDto>();
        
        stocks.ForEach(stock =>
        {
            var watchListDto = new WatchlistDto()
            {
                TickerSymbol = stock.TickerSymbol,
                ImgUrl = stock.ImgUrl,
                Name = stock.Name,
                MarketIdentifier = stock.MarketIdentifier
            };
            if (watchListDto.ImgUrl != null)
            {
                watchListDto.ImgUrl += $"&apiKey={_configuration["apiKey"]}";
            }

            watchlistToReturn.Add(watchListDto);
        });
        
        return new StatusResponse()
        {
            StatusCode = HttpStatusCode.OK,
            UserWatchlist = watchlistToReturn
        };
    }

    public async Task<StatusResponse> AddToWatchlistAsync(string username, string ticker)
    {
        var watchlist = await _context.Watchlists.Where(e => e.IdUserNavigation.EmailAddress == username).ToListAsync();

        var idUser = (await _context.Users.SingleOrDefaultAsync(e => e.EmailAddress == username))?.IdUser;

        if (watchlist.FirstOrDefault(e => e.TickerSymbol == ticker) == null && idUser != null)
        {
            _context.Watchlists.Add(new Watchlist()
            {
                IdUser = idUser.Value,
                TickerSymbol = ticker
            });
            await _context.SaveChangesAsync();
            return new StatusResponse()
            {
                StatusCode = HttpStatusCode.OK,
                Message = "Stock added to the watchlist"
            };
        }

        return new StatusResponse()
        {
            StatusCode = HttpStatusCode.BadRequest,
            Message = "Stock was already added to the watchlist"
        };
    }

    public async Task<StatusResponse> DeleteFromWatchlistAsync(string username, string ticker)
    {
        var watchlist = await _context.Watchlists
            .SingleOrDefaultAsync(e => e.IdUserNavigation.EmailAddress == username && e.TickerSymbol == ticker);

        if (watchlist == null)
        {
            return new StatusResponse()
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = "Watchlist not found"
            };
        }

        _context.Watchlists.Remove(watchlist);
        await _context.SaveChangesAsync();

        return new StatusResponse()
        {
            StatusCode = HttpStatusCode.OK,
            Message = "Stock deleted from watchlist"
        };
    }
}