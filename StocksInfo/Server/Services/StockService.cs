using System.Net;
using Microsoft.EntityFrameworkCore;
using Server.Entities;
using Shared.Models;
using Shared.Models.Aggregates;
using Result = Shared.Models.Aggregates.Result;

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


    public async Task<StatusResponse> GetStockAsync(string ticker)
    {
        ticker = ticker.ToUpper();
        var stock = await _context.Stocks.SingleOrDefaultAsync(e => e.TickerSymbol == ticker);

        if (stock != null)
        {
            return new StatusResponse()
            {
                StatusCode = HttpStatusCode.OK,
                Content = stock
            };
        }

        var url =
            $"https://api.polygon.io/v3/reference/tickers/{ticker}?apiKey={_configuration["apiKey"]}";

        var response = await _httpClient.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            var stockInfoDto = await response.Content.ReadFromJsonAsync<StockJsonModel>();

            stock = new Stock()
            {
                TickerSymbol = stockInfoDto.results.ticker,
                Name = stockInfoDto.results.name,
                PrimaryExchange = stockInfoDto.results.primary_exchange,
                Description = stockInfoDto.results.description,
                IndustrialClassification = stockInfoDto.results.sic_description,
                HomepageUrl = stockInfoDto.results.homepage_url,
            };

            if (stockInfoDto.results.list_date != null)
            {
                stock.ListDate = DateTime.Parse(stockInfoDto.results.list_date);
            }
            if (stockInfoDto.results.branding != null)
            {
                if (stockInfoDto.results.branding.logo_url != null)
                {
                    stock.ImgUrl = stockInfoDto.results.branding.logo_url;
                }
                else
                {
                    stock.ImgUrl = stockInfoDto.results.branding.icon_url;
                }

                stock.ImgUrl += $"?apiKey={_configuration["apiKey"]}";
            }

            await _context.Stocks.AddAsync(stock);
            await _context.SaveChangesAsync();
        }
        else
        {
            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                await Task.Delay(TimeSpan.FromSeconds(20));
                return await GetStockAsync(ticker);
            }

            var message = await response.Content.ReadAsStringAsync();

            return new StatusResponse()
            {
                StatusCode = response.StatusCode,
                Content = message
            };
        }

        return new StatusResponse()
        {
            StatusCode = HttpStatusCode.OK,
            Content = stock
        };
    }

    public async Task<StatusResponse> GetStocksSearchResultAsync(string token)
    {
        var url = "https://api.polygon.io/v3/reference/tickers?ticker.gte=" + token.ToUpper() +
                  $"&active=true&sort=ticker&order=asc&limit=5&apiKey={_configuration["apiKey"]}";

        var response = await _httpClient.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            var stockSearchResult = await response.Content.ReadFromJsonAsync<StocksSearchResultJsonModel>();

            var stocksFound = new List<FoundStockDto>();


            foreach (var stock in stockSearchResult.results)
            {
                stocksFound.Add(new FoundStockDto()
                {
                    TickerSymbol = stock.ticker,
                    Name = stock.name,
                    PrimaryExchange = stock.primary_exchange,
                });
            }

            return new StatusResponse()
            {
                StatusCode = HttpStatusCode.OK,
                Content = stocksFound
            };
        }

        if (response.StatusCode == HttpStatusCode.TooManyRequests)
        {
            await Task.Delay(TimeSpan.FromSeconds(20));
            return await GetStocksSearchResultAsync(token);
        }

        var message = await response.Content.ReadAsStringAsync();

        return new StatusResponse()
        {
            StatusCode = response.StatusCode,
            Content = message
        };
    }

    public async Task<StatusResponse> GetAggregatesAsync(string ticker)
    {
        var toDate = DateTime.Today.AddDays(-1);
        var fromDate = DateTime.Today.AddDays(-121);

        var aggregates = await _context
            .Aggregates.Where(e => e.TickerSymbol == ticker &&  e.Date > fromDate).ToListAsync();

        if (aggregates.Count > 0)
        {
            fromDate = aggregates.OrderByDescending(e => e.Date).FirstOrDefault().Date.AddDays(1);

            if (fromDate.AddDays(-1) == toDate)
            {
                return new StatusResponse()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = aggregates
                };
            }
        }

        var url =
            $"https://api.polygon.io/v2/aggs/ticker/{ticker}" +
            $"/range/1/day/" +
            $"{fromDate.ToString("yyyy-MM-dd")}/" +
            $"{toDate.ToString("yyyy-MM-dd")}" +
            $"?adjusted=false&sort=asc&limit=120&apiKey={_configuration["apiKey"]}";

        var response = await _httpClient.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            var stockSearchResult = await response.Content.ReadFromJsonAsync<StockAggregateJsonModel>();
            var newAggregates = new List<Aggregate>();
            if (stockSearchResult.resultsCount != 0)
            {
                newAggregates = stockSearchResult.results.ConvertAll(e => new Aggregate()
                    {
                        TickerSymbol = ticker,
                        Date = DateTimeOffset.FromUnixTimeMilliseconds(e.t).UtcDateTime,
                        Open = e.o,
                        Close = e.c,
                        High = e.h,
                        Low = e.l,
                        NumberOfTransactions = e.n,
                        // Volume = e.v,
                        AveragePrice = e.vw
                    }
                ).ToList();
                await _context.Aggregates.AddRangeAsync(newAggregates);
                await _context.SaveChangesAsync();
            }
            
            return new StatusResponse()
            {
                StatusCode = HttpStatusCode.OK,
                Content = aggregates.Concat(newAggregates).ToList()
            };
        }

        if (aggregates.Count != 0)
        {
            return new StatusResponse()
            {
                StatusCode = HttpStatusCode.OK,
                Content = aggregates
            };
        }
        
        if (response.StatusCode == HttpStatusCode.TooManyRequests)
        {
            await Task.Delay(TimeSpan.FromSeconds(20));
            return await GetAggregatesAsync(ticker);
        }

        return new StatusResponse()
        {
            StatusCode = response.StatusCode,
            Content = await response.Content.ReadAsStringAsync()
        };
    }
}

