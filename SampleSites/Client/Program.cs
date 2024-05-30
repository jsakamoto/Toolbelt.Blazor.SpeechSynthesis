using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SampleSite.Components;
using Toolbelt.Blazor.Extensions.DependencyInjection;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddSpeechSynthesis(options =>
{
    // options.DisableClientScriptAutoInjection = true;
});

await builder.Build().RunAsync(); ;
