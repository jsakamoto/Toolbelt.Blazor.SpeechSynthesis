# Blazor SpeechSynthesis [![NuGet Package](https://img.shields.io/nuget/v/Toolbelt.Blazor.SpeechSynthesis.svg)](https://www.nuget.org/packages/Toolbelt.Blazor.SpeechSynthesis/)

## Summary

This is a class library for Blazor app to provide Speech Synthesis API access.

## How to install and use?

**Step.1** Install the library via NuGet package, like this.

```shell
> dotnet add package Toolbelt.Blazor.SpeechSynthesis
```

**Step.2** Register "SpeechSynthesis" service into the DI container, at `ConfigureService` method in the `Startup` class of your Blazor application.

```csharp
using Toolbelt.Blazor.Extensions.DependencyInjection; // <- Add this, and...
...
public class Startup
{
  public void ConfigureServices(IServiceCollection services)
  {
    services.AddSpeechSynthesis(); // <- Add this line.
    ...
```

## License

[Mozilla Public License Version 2.0](https://github.com/jsakamoto/Toolbelt.Blazor.SpeechSynthesis/blob/master/LICENSE)