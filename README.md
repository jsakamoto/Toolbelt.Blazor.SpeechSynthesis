# Blazor SpeechSynthesis [![NuGet Package](https://img.shields.io/nuget/v/Toolbelt.Blazor.SpeechSynthesis.svg)](https://www.nuget.org/packages/Toolbelt.Blazor.SpeechSynthesis/)

## Summary

This is a class library for Blazor app (both "WebAssembly App" client-side model and "Server App" server-side model) to provide Speech Synthesis API access.

- [Live Demo Site](https://jsakamoto.github.io/Toolbelt.Blazor.SpeechSynthesis/)

## How to install and use?

### 1. Installation and Registration

**Step.1-1** Install the library via NuGet package, like this.

```shell
> dotnet add package Toolbelt.Blazor.SpeechSynthesis
```

**Step.1-2** Register "SpeechSynthesis" service into the DI container.

```csharp
// Program.cs
...
using Toolbelt.Blazor.Extensions.DependencyInjection; // <- Add this, and...
...
var builder = WebApplication.CreateDefault(args);
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

  async Task onClickSpeak() {
    await this.SpeechSynthesis.SpeakAsync(this.Text); // 👈 Speak!
  }
}
```

You can also speak with detail parameters, such as pitch, rate, volume, by using `SpeechSynthesisUtterance` object.

```csharp
  async Task onClickSpeak() {
    var utterancet = new SpeechSynthesisUtterance {
        Text = this.Text,
        Lang = "en-US", // BCP 47 language tag
        Pitch = 1.0, // 0.0 ~ 2.0 (Default 1.0)
        Rate = 1.0, // 0.1 ~ 10.0 (Default 1.0)
        Volume = 1.0 // 0.0 ~ 1.0 (Default 1.0)
    }
    await this.SpeechSynthesis.SpeakAsync(utterancet); // 👈 Speak!
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

  async Task onClickSpeak() {
    var utterancet = new SpeechSynthesisUtterance {
        Text = this.Text,
        Voice = this.Voices.FirstOrDefault(v => v.Name.Contains("Haruka"));
    }
    await this.SpeechSynthesis.SpeakAsync(utterancet); // 👈 Speak with "Haruka"'s voice!
  }
```

## JavaScript file cache busting

This library includes and uses a JavaScript file to access the browser's Speech Synthesis API. When you update this library to a newer version, the browser may use the cached previous version of the JavaScript file, leading to unexpected behavior. To prevent this issue, the library appends a version query string to the JavaScript file URL when loading it.

### .NET 8 and 9

A version query string will always be appended to this library's JavaScript file URL regardless of the Blazor hosting model you are using.

### .NET 10 or later

By default, a version query string will be appended to the JavaScript file URL that this library loads. If you want to disable appending a version query string to the JavaScript file URL, you can do so by setting the `TOOLBELT_BLAZOR_SPEECHSYNTHESIS_JSCACHEBUSTING` environment variable to `0`.

```csharp
// Program.cs
...
// 👇 Add this line to disable appending a version query string for this library's JavaScript file.
Environment.SetEnvironmentVariable("TOOLBELT_BLAZOR_SPEECHSYNTHESIS_JSCACHEBUSTING", "0");

var builder = WebApplication.CreateBuilder(args);
...
```

**However,** when you publish a .NET 10 Blazor WebAssembly app, a version query string will always be appended to the JavaScript file URL regardless of the `TOOLBELT_BLAZOR_SPEECHSYNTHESIS_JSCACHEBUSTING` environment variable setting. The reason is that published Blazor WebAssembly standalone apps don't include import map entries for JavaScript files from NuGet packages. If you want to avoid appending a version query string to the JavaScript file URL in published Blazor WebAssembly apps, you need to set the `ToolbeltBlazorSpeechSynthesisJavaScriptCacheBusting` MSBuild property to `false` in the project file of the Blazor WebAssembly app, like this:

```xml
<PropertyGroup>
  <ToolbeltBlazorSpeechSynthesisJavaScriptCacheBusting>false</ToolbeltBlazorSpeechSynthesisJavaScriptCacheBusting>
</PropertyGroup>
```

### Why do we append a version query string to the JavaScript file URL regardless of whether the import map is available or not?

We know that .NET 9 or later allows us to use import maps to import JavaScript files with a fingerprint in their file names. Therefore, in .NET 9 or later Blazor apps, you may want to avoid appending a version query string to the JavaScript file URL that this library loads.

However, we recommend keeping the default behavior of appending a version query string to the JavaScript file URL. The reason is that published Blazor WebAssembly standalone apps don't include import map entries for JavaScript files from NuGet packages. This inconsistent behavior between development and production environments and hosting models may lead to unexpected issues that are hard to diagnose, particularly in AutoRender mode apps.

## Release Note

Release notes is [here](https://github.com/jsakamoto/Toolbelt.Blazor.SpeechSynthesis/blob/master/RELEASE-NOTES.txt).

## License

[Mozilla Public License Version 2.0](https://github.com/jsakamoto/Toolbelt.Blazor.SpeechSynthesis/blob/master/LICENSE)