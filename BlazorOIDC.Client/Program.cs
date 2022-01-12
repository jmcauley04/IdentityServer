using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazorOIDC.Client;
using BlazorOIDC.Client.MessageHandlers;
using BlazorOIDC.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddTransient<OnDemandApiAuthMessageHandler>();

builder.Services.AddHttpClient<WeatherForecastService>(
    client => client.BaseAddress = new Uri(builder.Configuration.GetValue<string>("BlazorOIDCApi")))
    .AddHttpMessageHandler<OnDemandApiAuthMessageHandler>();

builder.Services.AddOidcAuthentication(options =>
    builder.Configuration.Bind("OidcConfiguration", options.ProviderOptions));

await builder.Build().RunAsync();
