using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Services;

namespace Server.Controllers;

[Route("api/[controller]")]
[ApiController]
// [Authorize]
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
        var response = await _stockService.GetStockAsync(ticker);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            return StatusCode((int) response.StatusCode, response.Message);
        }

        return Ok(response.StockDto);
    }

    [HttpGet]
    [Route("Search/{searchText}")]
    public async Task<IActionResult> GetStockSearchResult(string searchText)
    {
        var response = await _stockService.GetStocksSearchResultAsync(searchText);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            return StatusCode((int) response.StatusCode, response.Message);
        }

        return Ok(response.FoundStockDtos);
    }

    [HttpGet]
    [Route("{ticker}/Aggregates")]
    public async Task<IActionResult> GetStockAggregates(string ticker)
    {
        var response = await _stockService.GetAggregatesAsync(ticker);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            return StatusCode((int) response.StatusCode, response.Message);
        }

        return Ok(response.Aggregates);
    }
}