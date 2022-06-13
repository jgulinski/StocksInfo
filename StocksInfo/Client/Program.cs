using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Client;
using Client.Providers;
using Client.Services;
using IgniteUI.Blazor.Controls;
using Microsoft.AspNetCore.Components.Authorization;
using Syncfusion.Blazor;

// Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("NjU0OTkyQDMyMzAyZTMxMmUzMFNmalNubENzYnRmUW5IMWwzZmtoZFBRbzNhUnNMbHppbnZ0bzA5VzBDdkU9");

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// builder.Services.AddScoped(sp => new HttpClient {BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)});

builder.Services.AddSingleton(sp => new HttpClient {BaseAddress = new Uri("https://localhost:7013/")});

builder.Services.AddBlazoredLocalStorage();

builder.Services.AddAuthorizationCore();

builder.Services.AddScoped<AppAuthenticationStateProvider>();

builder.Services.AddScoped<AuthenticationStateProvider>(provider => 
    provider.GetRequiredService<AppAuthenticationStateProvider>());

builder.Services.AddScoped<IStockService, StockService>();

builder.Services.AddSyncfusionBlazor();

builder.Services.AddIgniteUIBlazor();

await builder.Build().RunAsync();