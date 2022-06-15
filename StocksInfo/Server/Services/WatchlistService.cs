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
        var watchlist = new List<WatchlistDto>();
        foreach (var stock in stocks)
        {
            var watchListDto = new WatchlistDto()
            {
                TickerSymbol = stock.TickerSymbol,
                ImgUrl = stock.ImgUrl,
                Name = stock.Name,
                PrimaryExchange = stock.PrimaryExchange
            };
            if (watchListDto.ImgUrl != null)
            {
                watchListDto.ImgUrl += $"&apiKey={_configuration["apiKey"]}";
            }

            watchlist.Add(watchListDto);
        }

        return new StatusResponse()
        {
            StatusCode = HttpStatusCode.OK,
            Content = watchlist
        };
    }

    public async Task<StatusResponse> AddToWatchlistAsync(string username, string ticker)
    {
        var watchlist = await _context.Watchlists.Where(e => e.IdUserNavigation.EmailAddress == username).ToListAsync();

        var idUser = (await _context.Users.SingleOrDefaultAsync(e => e.EmailAddress == username)).IdUser;

        if (watchlist.FirstOrDefault(e => e.Ticker == ticker) == null)
        {
            _context.Watchlists.Add(new Watchlist()
            {
                IdUser = idUser,
                Ticker = ticker
            });
            await _context.SaveChangesAsync();
            return new StatusResponse()
            {
                StatusCode = HttpStatusCode.OK,
                Content = "Stock added to the watchlist"
            };
        }

        return new StatusResponse()
        {
            StatusCode = HttpStatusCode.OK,
            Content = "Stock was already added to the watchlist"
        };
    }

    public async Task<StatusResponse> DeleteFromWatchlistAsync(string username, string ticker)
    {
        var watchlist = await _context.Watchlists
            .SingleOrDefaultAsync(e => e.IdUserNavigation.EmailAddress == username && e.Ticker == ticker);

        if (watchlist == null)
        {
            return new StatusResponse()
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = "Something went wrong"
            };
        }

        _context.Watchlists.Remove(watchlist);
        await _context.SaveChangesAsync();

        return new StatusResponse()
        {
            StatusCode = HttpStatusCode.OK,
            Content = "Stock deleted from watchlist"
        };
    }
}