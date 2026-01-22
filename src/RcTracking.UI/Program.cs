using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using RcTracking.UI;
using RcTracking.UI.Services;
using RcTracking.UI.Interface;

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

await builder.Build().RunAsync();
