using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PoOmad.Client;
using PoOmad.Client.Services;
using Radzen;
using Blazored.LocalStorage;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure HttpClient - uses same origin when hosted in API
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Add application services
builder.Services.AddScoped<ApiClient>();
builder.Services.AddSingleton<AuthStateService>();

// Add Radzen services
builder.Services.AddRadzenComponents();

// Add Blazored LocalStorage for offline support
builder.Services.AddBlazoredLocalStorage();

await builder.Build().RunAsync();
