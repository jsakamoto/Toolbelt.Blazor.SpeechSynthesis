using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;

namespace Toolbelt.Blazor.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for adding SpeechSynthesis service.
    /// </summary>
    public static class SpeechSynthesisExtensions
    {
        /// <summary>
        ///  Adds a SpeechSynthesis service to the specified Microsoft.Extensions.DependencyInjection.IServiceCollection.
        /// </summary>
        /// <param name="services">The Microsoft.Extensions.DependencyInjection.IServiceCollection to add the service to.</param>
        public static IServiceCollection AddSpeechSynthesis(this IServiceCollection services)
        {
            services.AddScoped(serviceProvider => new global::Toolbelt.Blazor.SpeechSynthesis.SpeechSynthesis(serviceProvider.GetService<IJSRuntime>()).Refresh());
            return services;
        }
    }
}