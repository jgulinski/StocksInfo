using Client.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Shared.Models;

namespace Client.Shared;

public class StocksBase : ComponentBase
{
    [Parameter] public string Ticker { get; set; }

    [Inject] public IStockService StockService { get; set; }

    protected Stock? Stock = null;
    protected bool isAuthenticated = false;

    protected string? ErrorMessage { get; set; }
    [CascadingParameter] protected Task<AuthenticationState> AuthenticationState { get; set; }

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var user = (await AuthenticationState).User;
            if (user.Identity.IsAuthenticated)
            {
                isAuthenticated = true;
                Stock = await StockService.GetStockInfo(Ticker);
            }
        }
        catch
            (Exception e)
        {
            ErrorMessage = e.Message;
        }
    }
}