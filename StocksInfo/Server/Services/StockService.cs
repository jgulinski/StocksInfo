using System.Net;
using Microsoft.EntityFrameworkCore;
using Server.Entities;
using Shared.Models;

namespace Server.Services;

public class StockService : IStockService
{
    private readonly ApiContext _context;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;


    public StockService(ApiContext context, IConfiguration configuration, HttpClient httpClient)
    {
        _context = context;
        _configuration = configuration;
        _httpClient = httpClient;
    }


    public async Task<Stock?> GetStockAsync(string ticker)
    {
        ticker = ticker.ToUpper();
        var stockInfo = await _context.Stocks.SingleOrDefaultAsync(e => e.TickerSymbol == ticker);

        if (stockInfo != null)
        {
            return stockInfo;
        }

        var url =
            $"https://api.polygon.io/v3/reference/tickers/{ticker}?apiKey={_configuration["apiKey"]}";

        var response = await _httpClient.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            // if (response.StatusCode == HttpStatusCode.NoContent)
            // {
            //     return null;
            // }
            var stockInfoDto = await response.Content.ReadFromJsonAsync<StockJsonModel>();

            stockInfo = new Stock()
            {
                TickerSymbol = stockInfoDto.results.ticker,
                Name = stockInfoDto.results.name,
                PrimaryExchange = stockInfoDto.results.primary_exchange,
                Description = stockInfoDto.results.description,
                IndustrialClassification = stockInfoDto.results.sic_description,
                HomepageUrl = stockInfoDto.results.homepage_url,
                ListDate = DateTime.Parse(stockInfoDto.results.list_date),
            };

            if (stockInfoDto.results.branding != null)
            {
                if (stockInfoDto.results.branding.logo_url != null)
                {
                    stockInfo.ImgUrl = stockInfoDto.results.branding.logo_url;
                }
                else
                {
                    stockInfo.ImgUrl = stockInfoDto.results.branding.icon_url;
                }
            }

            await _context.Stocks.AddAsync(stockInfo);
            await _context.SaveChangesAsync();
        }
        else
        {
            // var stockInfoDto = await response.Content.ReadFromJsonAsync<StockInfoDTO>();
            //
            //
            // if (stockInfoDto.error == requestLimitExceededText)
            // {
            //
            // }

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                await Task.Delay(TimeSpan.FromSeconds(20));
                return await GetStockAsync(ticker);
            }

            var message = await response.Content.ReadAsStringAsync();
            throw new Exception(message);
        }

        return stockInfo;
    }

    public async Task<List<FoundStockDto>> GetStocksSearchResultAsync(string token)
    {
        var url = "https://api.polygon.io/v3/reference/tickers?ticker.gte=" + token.ToUpper() +
                  $"&active=true&sort=ticker&order=asc&limit=5&apiKey={_configuration["apiKey"]}";


        var response = await _httpClient.GetAsync(url);


        if (response.IsSuccessStatusCode)
        {
            var stockSearchResult = response.Content.ReadFromJsonAsync<StocksSearchResultJsonModel>();

            var stocksFound = new List<FoundStockDto>();


            foreach (var stock in stockSearchResult.Result.results)
            {
                stocksFound.Add(new FoundStockDto()
                {
                    Ticker = stock.ticker,
                    Name = stock.name,
                    PrimaryExchange = stock.primary_exchange,
                });
            }

            return stocksFound;
        }

        if (response.StatusCode == HttpStatusCode.TooManyRequests)
        {
            await Task.Delay(TimeSpan.FromSeconds(20));
            return await GetStocksSearchResultAsync(token);
        }

        var message = await response.Content.ReadAsStringAsync();

        throw new Exception(message);
    }
}