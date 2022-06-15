using Client.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Shared.Models;

namespace Client.Shared;

public class StocksBase : ComponentBase
{
    [Parameter] public string Ticker { get; set; }

    [Inject] public IStockService StockService { get; set; }

    protected StockDto? Stock;
    protected bool IsAuthenticated;

    protected string? ErrorMessage { get; set; }
    [CascadingParameter] protected Task<AuthenticationState> AuthenticationState { get; set; }

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var user = (await AuthenticationState).User;
            if (user.Identity.IsAuthenticated)
            {
                IsAuthenticated = true;
                Stock = await StockService.GetStockInfoAsync(Ticker);
            }
        }
        catch
            (Exception e)
        {
            ErrorMessage = e.Message;
        }
    }
}