using Microsoft.AspNetCore.Components.Builder;
using Microsoft.Extensions.DependencyInjection;
using Toolbelt.Blazor.Extensions.DependencyInjection;

namespace ClientSideBlazorSampleSite
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSpeechSynthesis();
        }

        public void Configure(IComponentsApplicationBuilder app)
        {
            app.UseLocalTimeZone();
            app.AddComponent<App>("app");
        }
    }
}
