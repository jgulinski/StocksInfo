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
    
    [Inject]
    public IStockService StockService { get; set; }

    [Parameter]
    public string TickerSymbol { get; set; }
    [Parameter]
    public List<PriceChangeDto> PriceChanges { get; set; } 

    protected ConfirmBase DeleteConfirmation { get; set; }
    protected List<WatchlistDto>? Watchlist { get; set; }

    private string? ErrorMessage { get; set; }


    
    protected override async Task OnInitializedAsync()
    {
        try
        {
            Watchlist = await UserService.GetWatchlistAsync(Username);

            Watchlist.Where(e => e.ImgUrl == null)
                .ToList()
                .ForEach(e => 
                    e.ImgUrl = "https://www.freeiconspng.com/thumbs/no-image-icon/no-image-icon-15.png");
            
            
            var tickers = Watchlist.Select(e => e.TickerSymbol).ToList();

            PriceChanges = await StockService.GetStocksPriceChange(tickers);
        }
        catch (Exception e)
        {
            ErrorMessage = e.Message;
        }
    }
    

}