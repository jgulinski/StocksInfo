using Client.Services;
using Microsoft.AspNetCore.Components;
using Shared.Models;


namespace Client.Shared;

public class ListBase : ComponentBase
{
    [Parameter]
    public string Username { get; set; }

    [Inject]
    public IUserService UserService { get; set; }

    [Parameter]
    public string TickerSymbol { get; set; }

    protected ConfirmBase DeleteConfirmation { get; set; }
    protected List<WatchlistDto>? Watchlist { get; set; }

    private string? ErrorMessage { get; set; }


    
    protected override async Task OnInitializedAsync()
    {
        try
        {
            Watchlist = await UserService.GetWatchlistAsync(Username);
        }
        catch (Exception e)
        {
            ErrorMessage = e.Message;
        }
    }
    

}