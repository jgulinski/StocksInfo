using Client.Services;
using Microsoft.AspNetCore.Components;
using Shared.Models.Aggregates;

namespace Client.Shared;

public class ChartBase : ComponentBase
{
    [Parameter]
    public string Ticker { get; set; }

    [Inject]
    public IStockService StockService { get; set; }

    protected List<AggregateDto>? Aggregates { get; set; }

    protected string? ErrorMessage { get; set; }
    protected override async Task OnInitializedAsync()
    {
        try
        {
            Aggregates = await StockService.GetStockAggregatesAsync(Ticker);
        }
        catch (Exception e)
        {
            ErrorMessage = e.Message;
        }
    }
}