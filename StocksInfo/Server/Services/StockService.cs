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
            stockDto = Converters.StockToStockDtoConverter(stock);

            return new StatusResponse()
            {
                StatusCode = HttpStatusCode.OK,
                StockDto = stockDto
            };
        }

        var url = $"https://api.polygon.io/v3/reference/tickers/{ticker}?apiKey={_configuration["apiKey"]}";

        var response = await _httpClient.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            stock = await AddNewStockToContext(response);
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

        stockDto = Converters.StockToStockDtoConverter(stock);

        return new StatusResponse()
        {
            StatusCode = HttpStatusCode.OK,
            StockDto = stockDto
        };
    }

    public async Task<StatusResponse> GetStocksSearchResultAsync(string token)
    {
        var url =
            $"https://financialmodelingprep.com/api/v3/search?query={token}&limit=10&exchange=NASDAQ,NSE,NYSE,FOREX&apikey={_configuration["FMPapiKey"]}";

        var response = await _httpClient.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            var stockSearchResult = await response.Content.ReadFromJsonAsync<List<StocksSearchResultJsonModel>>();

            var stocksFoundToReturn = new List<FoundStockDto>();

            stockSearchResult?.ForEach(stock => stocksFoundToReturn.Add(new FoundStockDto()
            {
                TickerSymbol = stock.TickerSymbol,
                Name = stock.Name,
                StockExchange = stock.StockExchange,
            }));

            return new StatusResponse()
            {
                StatusCode = HttpStatusCode.OK,
                FoundStockDtos = stocksFoundToReturn
            };
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
            fromDate = aggregates.OrderByDescending(e => e.Date).First().Date;

            if (fromDate == toDate)
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

            fromDate = fromDate.AddDays(1);
        }

        var url =
            $"https://api.polygon.io/v2/aggs/ticker/{ticker}" +
            "/range/1/day/" +
            $"{fromDate.ToString("yyyy-MM-dd")}/" +
            $"{toDate.ToString("yyyy-MM-dd")}" +
            $"?adjusted=false&sort=asc&limit=120&apiKey={_configuration["apiKey"]}";

        var response = await _httpClient.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            var stockSearchResult = await response.Content.ReadFromJsonAsync<StockAggregateJsonModel>();

            var newAggregates = new List<Aggregate>();

            if (stockSearchResult != null && stockSearchResult.resultsCount != 0)
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

    public async Task<StatusResponse> GetPriceChangesAsync(string tickers)
    {
        var url =
            $"https://financialmodelingprep.com/api/v3/stock-price-change/{tickers}" +
            $"?apikey={_configuration["FMPapiKey"]}";

        var priceChanges = new List<PriceChangeDto>();

        var response = await _httpClient.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            priceChanges = await response.Content.ReadFromJsonAsync<List<PriceChangeDto>>();

            return new StatusResponse()
            {
                StatusCode = HttpStatusCode.OK,
                PriceChanges = priceChanges
            };
        }

        return new StatusResponse()
        {
            StatusCode = HttpStatusCode.NoContent,
            PriceChanges = priceChanges
        };
    }

    public async Task<StatusResponse> GetStockArticlesAsync(string ticker)
    {
        var url =
            $"https://api.polygon.io/v2/reference/news?ticker={ticker}&limit=5&apiKey={_configuration["apiKey"]}";

        var response = await _httpClient.GetAsync(url);

        var articleDtoList = new List<ArticleDto>();

        if (response.IsSuccessStatusCode)
        {
            var articles = (await response.Content.ReadFromJsonAsync<ArticlesJsonModel>())?.Results;


            articles?.ForEach(a => articleDtoList.Add(new ArticleDto()
                {
                    Publisher = a.Publisher.Name,
                    Title = a.Title,
                    Author = a.Author,
                    Published = a.Published,
                    Url = a.Url,
                    Description = a.Description
                }
            ));
            return new StatusResponse()
            {
                StatusCode = HttpStatusCode.OK,
                Articles = articleDtoList
            };
        }
        
        if (response.StatusCode == HttpStatusCode.TooManyRequests)
        {
            await Task.Delay(TimeSpan.FromSeconds(20));
            return await GetStockArticlesAsync(ticker);
        }

        var message = await response.Content.ReadAsStringAsync();


        return new StatusResponse()
        {
            StatusCode = HttpStatusCode.NotFound,
            Message = message
        };
    }

    private async Task<Stock> AddNewStockToContext(HttpResponseMessage response)
    {
        var stockJsonModel = await response.Content.ReadFromJsonAsync<StockJsonModel>();

        Stock stock = Converters.StockJsonModelToStockConverter(stockJsonModel);

        if (stockJsonModel.Results.Branding?.LogoUrl != null)
        {
            stock.ImgUrl = stockJsonModel.Results.Branding.LogoUrl;
            stock.ImgUrl += $"?apiKey={_configuration["apiKey"]}";
        }
        else if (stockJsonModel.Results.Branding?.IconUrl != null)
        {
            stock.ImgUrl = stockJsonModel.Results.Branding.IconUrl;
            stock.ImgUrl += $"?apiKey={_configuration["apiKey"]}";
        }

        await _context.Stocks.AddAsync(stock);
        await _context.SaveChangesAsync();

        return stock;
    }
}