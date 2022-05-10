# Blazor SpeechSynthesis [![NuGet Package](https://img.shields.io/nuget/v/Toolbelt.Blazor.SpeechSynthesis.svg)](https://www.nuget.org/packages/Toolbelt.Blazor.SpeechSynthesis/)

## Summary

This is a class library for Blazor app (both "WebAssembly App" client-side model and "Server App" server-side model) to provide Speech Synthesis API access.

- [Live Demo Site](https://jsakamoto.github.io/Toolbelt.Blazor.SpeechSynthesis//)

## How to install and use?

### 1. Installation and Registration

**Step.1-1** Install the library via NuGet package, like this.

```shell
> dotnet add package Toolbelt.Blazor.SpeechSynthesis
```

**Step.1-2** Register "SpeechSynthesis" service into the DI container.

If the project is a Blazor Server App or a Blazor WebAssembly App ver.3.1 Preview 4 or earlier, add the code into the `ConfigureService` method in the `Startup` class of your Blazor application.

```csharp
// Startup.cs

using Toolbelt.Blazor.Extensions.DependencyInjection; // <- Add this, and...
...
public class Startup
{
  public void ConfigureServices(IServiceCollection services)
  {
    services.AddSpeechSynthesis(); // <- Add this line.
    ...
```

If the project is a Blazor WebAssembly App ver.3.2 Preview 1 or later, add the code into the `Main` method in the `Program` class of your Blazor application.

```csharp
// Program.cs

using Toolbelt.Blazor.Extensions.DependencyInjection; // <- Add this, and...
...
public class Program
{
  public static async Task Main(string[] args)
  {
    var builder = WebAssemblyHostBuilder.CreateDefault(args);
    ...
    builder.Services.AddSpeechSynthesis(); // <- Add this line.
    ...
```

### 2. Usage in your Blazor component (.razor)

**Step.2-1**  Open the `Toolbelt.Blazor.SpeechSynthesis` namespace, and inject the `SpeechSynthesis` service into the component.

```csharp
@{/* This is your component .razor */}
@using Toolbelt.Blazor.SpeechSynthesis @{/* Add these two lines. */}
@inject SpeechSynthesis SpeechSynthesis
...
```

**Step.2-2** Invoke `Speak()` method of the `SpeechSynthesis` service instance to speak!

```html
@using Toolbelt.Blazor.SpeechSynthesis
@inject SpeechSynthesis SpeechSynthesis

<div> <textarea @bind="Text"></textarea> </div>

<div> <button @onclick="onClickSpeak">Speak</button> </div>

@code {

  string Text;

  void onClickSpeak() {
    this.SpeechSynthesis.Speak(this.Text); // <-- Speak!
  }
}
```

You can also speak with detail parameters, such as pitch, rate, volume, by using `SpeechSynthesisUtterance` object.

```csharp
  void onClickSpeak() {
    var utterancet = new SpeechSynthesisUtterance {
        Text = this.Text,
        Lang = "en-US", // BCP 47 language tag
        Pitch = 1.0, // 0.0 ~ 2.0 (Default 1.0)
        Rate = 1.0, // 0.1 ~ 10.0 (Default 1.0)
        Volume = 1.0 // 0.0 ~ 1.0 (Default 1.0)
    }
    this.SpeechSynthesis.Speak(utterancet); // <-- Speak!
  }
```

If you want to chose type of voices, you can do it with `GetVoicesAsync()` method of `SpeechSynthesis` service instance.

```csharp
  IEnumerable<SpeechSynthesisVoice> Voices;

  protected async override Task OnAfterRenderAsync(bool firstRender)
  {
    if (firstRender)
    {
      this.Voices = await this.SpeechSynthesis.GetVoicesAsync();
      this.StateHasChanged();
    }
  }

  void onClickSpeak() {
    var utterancet = new SpeechSynthesisUtterance {
        Text = this.Text,
        Voice = this.Voices.FirstOrDefault(v => v.Name.Contains("Haruka"));
    }
    this.SpeechSynthesis.Speak(utterancet); // <-- Speak with "Haruka"'s voice!
  }
```

## Release Note

Release notes is [here](https://github.com/jsakamoto/Toolbelt.Blazor.SpeechSynthesis/blob/master/RELEASE-NOTES.txt).

## License

[Mozilla Public License Version 2.0](https://github.com/jsakamoto/Toolbelt.Blazor.SpeechSynthesis/blob/master/LICENSE)