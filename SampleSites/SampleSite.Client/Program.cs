using System.Threading.Tasks;
using Microsoft.AspNetCore.Blazor.Hosting;
using SampleSite.Components;
using Toolbelt.Blazor.Extensions.DependencyInjection;

namespace SampleSite.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");
            builder.Services.AddSpeechSynthesis(options =>
            {
                // options.DisableClientScriptAutoInjection = true;
            });

            await builder
                .Build()
                .UseLocalTimeZone()
                .RunAsync(); ;
        }
    }
}
