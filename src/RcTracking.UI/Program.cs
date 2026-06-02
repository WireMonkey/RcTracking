using BlazorApplicationInsights;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using RcTracking.UI;
using RcTracking.UI.Interface;
using RcTracking.UI.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices();

var httpClient = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
builder.Services.AddScoped(sp => httpClient);

builder.Services.AddScoped<EventBus>();
builder.Services.AddScoped<IFlightService, FlightService>();
builder.Services.AddScoped<IPlaneService, PlaneService>();
builder.Services.AddScoped<ICombineDataService, CombineDataService>();
builder.Services.AddScoped<IImageService, ImageService>();

builder.Services.AddMsalAuthentication(options =>
{
    builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
});

var appInsights = builder.Configuration.GetConnectionString("appInsights");
builder.Services.AddBlazorApplicationInsights(config =>
{
    config.ConnectionString = appInsights;
});

await builder.Build().RunAsync();
