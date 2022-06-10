using Client.Services;
using Microsoft.AspNetCore.Components;
using Shared.Models;

namespace Client.Shared;

public class StocksBase : ComponentBase
{
    [Parameter]
    public string Ticker { get; set; }

    [Inject]
    public IStockService StockService { get; set; }

    protected Stock? Stock { get; set; }

    protected string? ErrorMessage { get; set; }

    protected override async Task OnInitializedAsync()
    {
        try
        {
            Stock = await StockService.GetStockInfo(Ticker);
        }
        catch (Exception e)
        {
            ErrorMessage = e.Message;
        }
    }
}