using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Toolbelt.Blazor.SpeechSynthesis;

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
        /// <param name="configure">An System.Action`1 to configure the options for SpeechSynthesis service.</param>
        public static IServiceCollection AddSpeechSynthesis(this IServiceCollection services, Action<SpeechSynthesisOptions> configure = null)
        {
            return services.AddScoped(serviceProvider =>
            {
                var options = new SpeechSynthesisOptions();
                configure?.Invoke(options);
                var jsRuntime = serviceProvider.GetService<IJSRuntime>();
                return new SpeechSynthesis.SpeechSynthesis(jsRuntime, options).Refresh();
            });
        }
    }
}