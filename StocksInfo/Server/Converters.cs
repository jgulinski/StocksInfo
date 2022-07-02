namespace Server;
using Shared.Models;

public static class Converters
{
    public static StockDto StockToStockDtoConverter(Stock stock)
    {
        var stockDto = new StockDto()
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
        return stockDto;
    }

    public static Stock StockJsonModelToStockConverter(StockJsonModel stockJsonModel)
    {
        var stock = new Stock()
        {
            TickerSymbol = stockJsonModel.Results.Ticker,
            Name = stockJsonModel.Results.Name,
            MarketIdentifier = stockJsonModel.Results.MarketIdentifier,
            Description = stockJsonModel.Results.Description,
            IndustrialClassification = stockJsonModel.Results.IndustrialClassification,
            HomepageUrl = stockJsonModel.Results.HomepageUrl,
        };

        if (stockJsonModel.Results.ListDate != null)
        {
            stock.ListDate = DateTime.Parse(stockJsonModel.Results.ListDate);
        }

        return stock;
    }
}