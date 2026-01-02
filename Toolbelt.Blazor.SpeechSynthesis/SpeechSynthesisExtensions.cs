using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Toolbelt.Blazor.SpeechSynthesis.Internals;

namespace Toolbelt.Blazor.Extensions.DependencyInjection;

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
        return services.AddScoped(serviceProvider =>
        {
            var jsRuntime = serviceProvider.GetRequiredService<IJSRuntime>();
            var logger = serviceProvider.GetRequiredService<ILogger<SpeechSynthesis.SpeechSynthesis>>();
            var speechSynthesis = new SpeechSynthesis.SpeechSynthesis(jsRuntime, logger);
            if (jsRuntime is IJSInProcessRuntime) speechSynthesis.GetStatusAsync().AsTask().WithLogException(logger);
            return speechSynthesis;
        });
    }
}