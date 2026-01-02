using Microsoft.AspNetCore.Components;
using Toolbelt.Blazor.SpeechSynthesis;

namespace SampleSite.Components.Pages;

public partial class Index
{
    [SupplyParameterFromQuery, Parameter]
    public string? Text { get; set; }

    [SupplyParameterFromQuery, Parameter]
    public string? VoiceId { get; set; }

    [SupplyParameterFromQuery, Parameter]
    public string? Lang { get; set; }

    private double Pitch = 1.0;

    private double Rate = 1.0;

    private double Volume = 1.0;

    private IEnumerable<SpeechSynthesisVoice> Voices = Enumerable.Empty<SpeechSynthesisVoice>();

    private List<string> Logs = new();

    private readonly SpeechSynthesisUtterance CachedUtterancet = new();

    private bool Available;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        this.Available = await this.SpeechSynthesis.AvailableAsync;

        if (firstRender)
        {
            if (string.IsNullOrEmpty(this.Text)) this.Text = "Hello, World!";
            this.UpdateUrl();

            this.Voices = await this.SpeechSynthesis.GetVoicesAsync();
            this.StateHasChanged();

            this.SpeechSynthesis.UtteranceStarted += this.OnStart;
            this.SpeechSynthesis.UtteranceReachedBoundary += this.OnBoundary;
            this.SpeechSynthesis.UtteranceReachedMark += this.OnMark;
            this.SpeechSynthesis.UtterancePaused += this.OnPause;
            this.SpeechSynthesis.UtteranceResumed += this.OnResume;
            this.SpeechSynthesis.UtteranceEnded += this.OnEnd;
            this.SpeechSynthesis.UtteranceError += this.OnError;
        }
    }

    private void UpdateUrl()
    {
        var url = this.NavigationManager.GetUriWithQueryParameters(new Dictionary<string, object?>
        {
            [nameof(this.Text)] = this.Text,
            [nameof(this.VoiceId)] = this.VoiceId,
            [nameof(this.Lang)] = this.Lang,
        });
        this.NavigationManager.NavigateTo(url, replace: true);
    }

    private SpeechSynthesisVoice? GetVoice()
    {
        return this.Voices.FirstOrDefault(v => v.VoiceIdentity == this.VoiceId);
    }
    void OnInputText(ChangeEventArgs args)
    {
        this.Text = args.Value?.ToString() ?? "";
        this.UpdateUrl();
    }

    void OnChangeVoice(ChangeEventArgs args)
    {
        this.VoiceId = args.Value?.ToString();
        this.Lang = this.GetVoice()?.Lang ?? "";
        this.UpdateUrl();
    }

    async Task OnClickSpeakButton()
    {
        var utterancet = new SpeechSynthesisUtterance();
        this.SetupUtterancet(utterancet);
        await this.SpeechSynthesis.SpeakAsync(utterancet);
    }

    async Task OnClickSpeakByCachedButton()
    {
        this.SetupUtterancet(this.CachedUtterancet);
        await this.SpeechSynthesis.SpeakAsync(this.CachedUtterancet);
    }

    void SetupUtterancet(SpeechSynthesisUtterance utterancet)
    {
        utterancet.Text = this.Text;
        utterancet.Lang = this.Lang;
        utterancet.Pitch = this.Pitch;
        utterancet.Rate = this.Rate;
        utterancet.Volume = this.Volume;
        utterancet.Voice = this.GetVoice();
    }

    async Task OnClickPauseButton()
    {
        this.WriteLog("OnClickPauseButton");
        Console.WriteLine($"Speaking is [{this.SpeechSynthesis.Speaking}]");
        if (this.SpeechSynthesis.Speaking)
        {
            await this.SpeechSynthesis.PauseAsync();
        }
    }

    async Task OnClickResumeButton()
    {
        this.WriteLog("OnClickResumeButton");
        Console.WriteLine($"Paused is [{this.SpeechSynthesis.Paused}]");
        if (this.SpeechSynthesis.Paused)
        {
            await this.SpeechSynthesis.ResumeAsync();
        }
    }

    async Task OnClickCancelButton()
    {
        await this.SpeechSynthesis.CancelAsync();
    }

    void OnClickGC()
    {
        GC.Collect();
        this.WriteLog("GC");
    }

    void OnClickClearLog()
    {
        this.Logs.Clear();
        this.StateHasChanged();
    }

    void OnStart(object? sender, EventArgs args) { this.WriteLog("ON START!"); }
    void OnBoundary(object? sender, EventArgs args) { this.WriteLog("ON BOUNDARY!"); }
    void OnMark(object? sender, EventArgs args) { this.WriteLog("ON MARK!"); }
    void OnPause(object? sender, EventArgs args) { this.WriteLog("ON PAUSE!"); }
    void OnResume(object? sender, EventArgs args) { this.WriteLog("ON RESUME!"); }
    void OnEnd(object? sender, EventArgs args) { this.WriteLog("ON END!"); }
    void OnError(object? sender, EventArgs args) { this.WriteLog("ON ERROR!"); }

    void WriteLog(string text)
    {
        this.Logs = new[] { $"{DateTime.Now:HH:mm:ss.fff} - {text}" }.Concat(this.Logs).Take(20).ToList();
        this.StateHasChanged();
    }
}
