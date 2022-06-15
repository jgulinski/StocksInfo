using Microsoft.AspNetCore.Components;

namespace Client.Shared;

public class ConfirmBase : ComponentBase
{
    protected string Ticker { get; set; }
    protected bool ShowConfirmation { get; set; }
    
    public void Show(string ticker)
    {
        Ticker = ticker;
        ShowConfirmation = true;
        StateHasChanged();
    }

    [Parameter]
    public EventCallback<bool> ConfirmationChanged { get; set; }
    protected async Task OnConfirmationChange(bool value)
    {
        ShowConfirmation = false;
        await ConfirmationChanged.InvokeAsync(value);
    }
}