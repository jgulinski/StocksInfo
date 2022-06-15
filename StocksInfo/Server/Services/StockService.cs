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
        StockDto stockDto;

        if (stock != null)
        {
            stockDto = new StockDto()
            {
                Description = stock.Description,
                HomepageUrl = stock.HomepageUrl,
                ImgUrl = stock.ImgUrl,
                IndustrialClassification = stock.IndustrialClassification,
                ListDate = stock.ListDate,
                PrimaryExchange = stock.PrimaryExchange,
                TickerSymbol = stock.TickerSymbol
            };
            return new StatusResponse()
            {
                StatusCode = HttpStatusCode.OK,
                Content = stockDto
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
                TickerSymbol = stockInfoDto.Results.Ticker,
                Name = stockInfoDto.Results.Name,
                PrimaryExchange = stockInfoDto.Results.PrimaryExchange,
                Description = stockInfoDto.Results.Description,
                IndustrialClassification = stockInfoDto.Results.IndustrialClassification,
                HomepageUrl = stockInfoDto.Results.HomepageUrl,
            };

            if (stockInfoDto.Results.ListDate != null)
            {
                stock.ListDate = DateTime.Parse(stockInfoDto.Results.ListDate);
            }
            if (stockInfoDto.Results.Branding != null)
            {
                if (stockInfoDto.Results.Branding.LogoUrl != null)
                {
                    stock.ImgUrl = stockInfoDto.Results.Branding.LogoUrl;
                }
                else
                {
                    stock.ImgUrl = stockInfoDto.Results.Branding.IconUrl;
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
        
        stockDto = new StockDto()
        {
            TickerSymbol = stock.TickerSymbol,
            Name = stock.Name,
            PrimaryExchange = stock.PrimaryExchange,
            Description = stock.Description,
            IndustrialClassification = stock.IndustrialClassification,
            HomepageUrl = stock.HomepageUrl,
            ListDate = stock.ListDate,
            ImgUrl = stock.ImgUrl
        };

        return new StatusResponse()
        {
            StatusCode = HttpStatusCode.OK,
            Content = stockDto
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


            foreach (var stock in stockSearchResult.Results)
            {
                stocksFound.Add(new FoundStockDto()
                {
                    TickerSymbol = stock.Ticker,
                    Name = stock.Name,
                    PrimaryExchange = stock.PrimaryExchange,
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

        var aggregateDtos = new List<AggregateDto>();

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
                    Date = DateTimeOffset.FromUnixTimeMilliseconds(e.Time).UtcDateTime,
                    Open = e.Open,
                    Close = e.Close,
                    High = e.High,
                    Low = e.Low,
                    NumberOfTransactions = e.NumberOfTransactions,
                    Volume = e.Volume,
                    AveragePrice = e.AveragePrice
                }
                ).ToList();
                await _context.Aggregates.AddRangeAsync(newAggregates);
                await _context.SaveChangesAsync();
            }
            foreach (var aggregate in aggregates.Concat(newAggregates))
            {
                aggregateDtos.Add( new ()
                {
                    AveragePrice = aggregate.AveragePrice,
                    Close = aggregate.Close,
                    Date = aggregate.Date,
                    High = aggregate.High,
                    Low = aggregate.Low,
                    NumberOfTransactions = aggregate.NumberOfTransactions,
                    Open = aggregate.Open,
                    Volume = aggregate.Volume,
                    TickerSymbol = aggregate.TickerSymbol
                });
            }
            
            return new StatusResponse()
            {
                StatusCode = HttpStatusCode.OK,
                Content = aggregates.Concat(newAggregates).ToList()
            };
        }
        
        foreach (var aggregate in aggregates)
        {
            aggregateDtos.Add( new ()
            {
                AveragePrice = aggregate.AveragePrice,
                Close = aggregate.Close,
                Date = aggregate.Date,
                High = aggregate.High,
                Low = aggregate.Low,
                NumberOfTransactions = aggregate.NumberOfTransactions,
                Open = aggregate.Open,
                TickerSymbol = aggregate.TickerSymbol
            });
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

