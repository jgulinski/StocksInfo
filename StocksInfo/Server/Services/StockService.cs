using System.Net;
using Microsoft.EntityFrameworkCore;
using Server.Entities;
using Shared.Models;
using Shared.Models.Aggregates;

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
                Name = stock.Name,
                Description = stock.Description,
                HomepageUrl = stock.HomepageUrl,
                ImgUrl = stock.ImgUrl,
                IndustrialClassification = stock.IndustrialClassification,
                ListDate = stock.ListDate,
                MarketIdentifier = stock.MarketIdentifier,
                TickerSymbol = stock.TickerSymbol
            };
            return new StatusResponse()
            {
                StatusCode = HttpStatusCode.OK,
                StockDto = stockDto
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
                MarketIdentifier = stockInfoDto.Results.MarketIdentifier,
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
                Message = message
            };
        }

        stockDto = new StockDto()
        {
            TickerSymbol = stock.TickerSymbol,
            Name = stock.Name,
            MarketIdentifier = stock.MarketIdentifier,
            Description = stock.Description,
            IndustrialClassification = stock.IndustrialClassification,
            HomepageUrl = stock.HomepageUrl,
            ListDate = stock.ListDate,
            ImgUrl = stock.ImgUrl
        };

        return new StatusResponse()
        {
            StatusCode = HttpStatusCode.OK,
            StockDto = stockDto
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

            var stocksFoundToReturn = new List<FoundStockDto>();


            stockSearchResult.Results.ForEach(stock => stocksFoundToReturn.Add(new FoundStockDto()
            {
                TickerSymbol = stock.Ticker,
                Name = stock.Name,
                PrimaryExchange = stock.PrimaryExchange,
            }));

            return new StatusResponse()
            {
                StatusCode = HttpStatusCode.OK,
                FoundStockDtos = stocksFoundToReturn
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
            Message = message
        };
    }

    public async Task<StatusResponse> GetAggregatesAsync(string ticker)
    {
        var toDate = DateTime.Today.AddDays(-1);
        var fromDate = DateTime.Today.AddDays(-121);

        var aggregatesToReturn = new List<AggregateDto>();

        var aggregates = await _context
            .Aggregates.Where(e => e.TickerSymbol == ticker && e.Date > fromDate).ToListAsync();

        if (aggregates.Count > 0)
        {
            fromDate = aggregates.OrderByDescending(e => e.Date).First().Date.AddDays(1);

            if (fromDate.AddDays(-1) == toDate)
            {
                aggregates.ForEach(aggregate => aggregatesToReturn.Add(new AggregateDto()
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
                }));

                return new StatusResponse()
                {
                    StatusCode = HttpStatusCode.OK,
                    Aggregates = aggregatesToReturn
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

            aggregates.Concat(newAggregates).ToList().ForEach(aggregate => aggregatesToReturn.Add(new AggregateDto()
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
            }));

            return new StatusResponse()
            {
                StatusCode = HttpStatusCode.OK,
                Aggregates = aggregatesToReturn.ToList()
            };
        }

        aggregates.ForEach(aggregate => aggregatesToReturn.Add(new AggregateDto()
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
        }));

        if (aggregates.Count != 0)
        {
            return new StatusResponse()
            {
                StatusCode = HttpStatusCode.OK,
                Aggregates = aggregatesToReturn
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
            Message = await response.Content.ReadAsStringAsync()
        };
    }
}