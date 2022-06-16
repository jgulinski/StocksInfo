using Blazored.LocalStorage;
using Client.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.IdentityModel.JsonWebTokens;
using Shared.Models;

namespace Client.Shared;

public class StocksBase : ComponentBase
{
    [Parameter] public string Ticker { get; set; }
    [Parameter] public List<WatchlistDto> Watchlist { get; set; }

    [Inject] public IStockService StockService { get; set; }

    [Inject] public IUserService UserService { get; set; }
    
    protected string Username { get; set; }

    protected StockDto? Stock;
    protected bool IsAuthenticated;

    protected string? ErrorMessage { get; set; }
    [CascadingParameter] protected Task<AuthenticationState> AuthenticationState { get; set; }

    [Inject] AuthenticationStateProvider AuthenticationStateProvider { get; set; }

    [Inject] ILocalStorageService _localStorageService { get; set; }


    private bool _isAuthenticated;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var user = (await AuthenticationState).User;
            if (user.Identity.IsAuthenticated)
            {
                IsAuthenticated = true;

                if (await _localStorageService.ContainKeyAsync("bearerToken"))
                {
                    var savedToken = await _localStorageService.GetItemAsync<string>("bearerToken");

                    var handler = new JsonWebTokenHandler();
                    Username = handler.ReadJsonWebToken(savedToken).Subject;
                    
                    Watchlist = await UserService.GetWatchlistAsync(Username);

                }

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