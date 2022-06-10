using Microsoft.AspNetCore.Mvc;
using Server.Services;

namespace Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class StocksController : ControllerBase
{
    private readonly IStockService _stockService;

    public StocksController(IStockService stockService)
    {
        _stockService = stockService;
    }
    
    [HttpGet]
    [Route("{ticker}")]
    public async Task<IActionResult> GetStockInfo(string ticker)
    {
        var stockInfo = await _stockService.GetStockAsync(ticker);

        if (stockInfo == null)
        {
            return NotFound();
        }
        return Ok(stockInfo);
    }

    [HttpGet]
    [Route("Search/{searchText}")]
    public async Task<IActionResult> GetStockSearchResult(string searchText)
    {
        var stocks = await _stockService.GetStocksSearchResultAsync(searchText);
        return Ok(stocks);
    }
    
}