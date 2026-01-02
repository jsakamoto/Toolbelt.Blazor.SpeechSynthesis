using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Toolbelt.Blazor.SpeechSynthesis.Internals;

namespace Toolbelt.Blazor.SpeechSynthesis;

/// <summary>
/// The controller interface for the speech service of the Web Speech API.
/// </summary>
public class SpeechSynthesis : IAsyncDisposable
{
    private readonly IJSRuntime JSRuntime;

    private readonly ILogger Logger;

    private readonly SemaphoreSlim Syncer = new SemaphoreSlim(1, 1);

    private SpeechSynthesisStatus? _StatusCache = null;

    private readonly List<SpeechSynthesisVoice> _Voices = new();

    private readonly object _EventHandledUtterancesLock = new object();

    private WeakReference<SpeechSynthesisUtterance>[] _EventHandledUtterances = new WeakReference<SpeechSynthesisUtterance>[0];

    /// <summary>
    /// Gets a value that indicates whether the Web Speech API is available or not.
    /// </summary>
    public ValueTask<bool> AvailableAsync => this.GetStatusAsync(s => s.Available);

    #region DEPRECATED

    /// <summary>
    /// [Deprecated] use <see cref="AvailableAsync"/> or <see cref="GetStatusAsync"/> instead.
    /// </summary>
    [Obsolete("use \"AvailableAsync\" or \"GetStatusAsync\" instead."), EditorBrowsable(EditorBrowsableState.Never)]
    public bool Available => this._StatusCache?.Available ?? false;

    #endregion

    /// <summary>
    /// Gets a value that indicates whether the SpeechSynthesis object is in a paused state or not.
    /// </summary>
    public bool Paused => this._StatusCache?.Paused ?? false;

    /// <summary>
    /// Gets a value that indicates whether the utterance queue contains as-yet-unspoken utterances or not.
    /// </summary>
    public bool Pending => this._StatusCache?.Pending ?? false;

    /// <summary>
    /// Gets a value that indicates whether an utterance is currently in the process of being spoken (includes SpeechSynthesis is in a paused state) or not.
    /// </summary>
    public bool Speaking => this._StatusCache?.Speaking ?? false;

    /// <summary>
    /// Occurs when the utterance has begun to be spoken.
    /// </summary>
    public event EventHandler? UtteranceStarted;

    /// <summary>
    /// Occurs when the spoken utterance reaches a word or sentence boundary.
    /// </summary>
    public event EventHandler? UtteranceReachedBoundary;

    /// <summary>
    /// Occurs when the spoken utterance reaches a named SSML "mark" tag.
    /// </summary>
    public event EventHandler? UtteranceReachedMark;

    /// <summary>
    /// Occurs when the utterance is paused part way through.
    /// </summary>
    public event EventHandler? UtterancePaused;

    /// <summary>
    /// Occurs when a paused utterance is resumed.
    /// </summary>
    public event EventHandler? UtteranceResumed;

    /// <summary>
    /// Occurs when the utterance has finished being spoken.
    /// </summary>
    public event EventHandler? UtteranceEnded;

    /// <summary>
    /// Occurs when an error occurs that prevents the utterance from being succesfully spoken.
    /// </summary>
    public event EventHandler? UtteranceError;

    /// <summary>
    /// Initialize a new instance of the SpeechSynthesis class.
    /// </summary>
    internal SpeechSynthesis(IJSRuntime jSRuntime, ILogger logger)
    {
        this.JSRuntime = jSRuntime;
        this.Logger = logger;
    }

    /// <summary>
    /// Get the current status of the Web Speech API SpeechSynthesis object.
    /// </summary>
    /// <returns>Current status of the Web Speech API SpeechSynthesis object.</returns>
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(SpeechSynthesisStatus))]
    public async ValueTask<SpeechSynthesisStatus> GetStatusAsync()
    {
        this._StatusCache = await this.InvokeJavaScriptAsync<SpeechSynthesisStatus>("getStatus");
        return this._StatusCache;
    }

    private async ValueTask<T> GetStatusAsync<T>(Func<SpeechSynthesisStatus, T> selector)
    {
        var stat = await this.GetStatusAsync();
        return selector(stat);
    }

    private void UpdateStatus(SpeechSynthesisStatusEventArgs args)
    {
        this._StatusCache = args.Status;
    }

    private bool IsInProcessRuntime() => this.JSRuntime is IJSInProcessRuntime;

    /// <summary>
    /// Returns a collection of SpeechSynthesisVoice objects representing all the available voices on the current device.
    /// </summary>
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(SpeechSynthesisVoiceInternal))]
    public async Task<IReadOnlyCollection<SpeechSynthesisVoice>> GetVoicesAsync()
    {
        var latestVoices = this.IsInProcessRuntime() ?
            // If the app is running in the Blazor WebAssembly, we can get all voices at once
            // because there is no limit of the size of the data that can be passed between C# and JavaScript.
            await this.InvokeJavaScriptAsync<SpeechSynthesisVoiceInternal[]>("getVoices")
            // Otherwise, it means the app is running in the Blazor Server, we need to get voices in chunks because
            // there is a limit of the size of the data that can be passed between C# and JavaScript via the SignalR connection.
            : await this.GetLatestVoiceAsync();

        var toAddVoices = latestVoices.Where(p1 => !this._Voices.Any(p2 => p1.VoiceIdentity == p2.VoiceIdentity)).ToArray();
        var toRemoveVoices = this._Voices.Where(p1 => !latestVoices.Any(p2 => p1.VoiceIdentity == p2.VoiceIdentity)).ToArray();

        this._Voices.AddRange(toAddVoices.Select(v => new SpeechSynthesisVoice(v)));
        foreach (var voice in toRemoveVoices) this._Voices.Remove(voice);

        return this._Voices;
    }

    /// <summary>
    /// Returns a collection of <see cref="SpeechSynthesisVoiceInternal"/> object representing latest all the available voices on the current device.
    /// </summary>
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(SpeechSynthesisVoiceInternal))]
    private async ValueTask<IEnumerable<SpeechSynthesisVoiceInternal>> GetLatestVoiceAsync()
    {
        var numberOfVoices = await this.InvokeJavaScriptAsync<int>("getNumberOfVoices");
        var voices = new List<SpeechSynthesisVoiceInternal>(numberOfVoices);
        const int sizeofChunk = 100;
        for (var i = 0; i < numberOfVoices; i += sizeofChunk)
        {
            var voicesChunk = await this.InvokeJavaScriptAsync<SpeechSynthesisVoiceInternal[]>("getVoices", new { Begin = i, End = i + sizeofChunk });
            voices.AddRange(voicesChunk);
        }
        return voices;
    }

    #region DEPRECATED

    /// <summary>
    /// [Deprecated] Use <see cref="SpeakAsync(string)"/> instead.
    /// </summary>
    [Obsolete("Use \"SpeakAsync()\" instead."), EditorBrowsable(EditorBrowsableState.Never)]
    public void Speak(string text) => this.SpeakAsync(text).AsTask().WithLogException(this.Logger);

    /// <summary>
    /// [Deprecated] Use <see cref="SpeakAsync(SpeechSynthesisUtterance)"/> instead.
    /// </summary>
    [Obsolete("Use \"SpeakAsync()\" instead."), EditorBrowsable(EditorBrowsableState.Never)]
    public void Speak(SpeechSynthesisUtterance utterance) => this.SpeakAsync(utterance).AsTask().WithLogException(this.Logger);

    /// <summary>
    /// [Deprecated] Use <see cref="CancelAsync"/> instead.
    /// </summary>
    [Obsolete("Use \"CancelAsync\" instead."), EditorBrowsable(EditorBrowsableState.Never)]
    public void Cancel() { this.CancelAsync().AsTask().WithLogException(this.Logger); }

    /// <summary>
    /// [Deprecated] Use <see cref="PauseAsync"/> instead.
    /// </summary>
    [Obsolete("Use \"PauseAsync\" instead."), EditorBrowsable(EditorBrowsableState.Never)]
    public void Pause() { this.PauseAsync().AsTask().WithLogException(this.Logger); }

    /// <summary>
    /// [Deprecated] Use <see cref="ResumeAsync"/> instead.
    /// </summary>
    [Obsolete("Use \"ResumeAsync\" instead."), EditorBrowsable(EditorBrowsableState.Never)]
    public void Resume() { this.ResumeAsync().AsTask().WithLogException(this.Logger); }

    #endregion

    /// <summary>
    /// Adds an utterance initialized with specified text to the utterance queue.
    /// <para>it will be spoken when any other utterances queued before it have been spoken.</para>
    /// </summary>
    public ValueTask SpeakAsync(string text) => this.SpeakAsync(new SpeechSynthesisUtterance { Text = text });

    /// <summary>
    /// Adds an utterance to the utterance queue.
    /// <para>it will be spoken when any other utterances queued before it have been spoken.</para>
    /// </summary>
    public async ValueTask SpeakAsync(SpeechSynthesisUtterance utterance)
    {
        if (this._StatusCache == null) await this.GetStatusAsync();

        if (this._StatusCache!.Available)
        {
            lock (this._EventHandledUtterancesLock)
            {
                var eventHandledUtterances = this._EventHandledUtterances
                    .Select(wref => (WeakRef: wref, Utterance: wref.TryGetTarget(out var u) ? u : null))
                    .Where(item => item.Utterance != null)
                    .ToArray();
                var eventHandled = eventHandledUtterances.Any(item => Object.ReferenceEquals(item.Utterance, utterance));

                if (!eventHandled)
                {
                    eventHandledUtterances = eventHandledUtterances
                        .ToArray();
                    this._EventHandledUtterances = eventHandledUtterances.Select(item => item.WeakRef)
                        .Concat(new[] { new WeakReference<SpeechSynthesisUtterance>(utterance) })
                        .ToArray();
                    this.HandleEvents(utterance);
                }
                else
                    this._EventHandledUtterances = eventHandledUtterances.Select(item => item.WeakRef).ToArray();
            }

            this._StatusCache = await this.InvokeJavaScriptAsync<SpeechSynthesisStatus>("speak", utterance, utterance.GetObjectRef());
        }
    }

    /// <summary>
    /// Removes all utterances from the utterance queue.
    /// </summary>
    public async ValueTask CancelAsync()
    {
        this._StatusCache = await this.InvokeJavaScriptAsync<SpeechSynthesisStatus>("cancel");
    }

    /// <summary>
    /// Puts the SpeechSynthesis object into a paused state.
    /// </summary>
    public async ValueTask PauseAsync()
    {
        this._StatusCache = await this.InvokeJavaScriptAsync<SpeechSynthesisStatus>("pause");
    }

    /// <summary>
    /// Puts the SpeechSynthesis object into a non-paused state if it was already paused.
    /// </summary>
    public async ValueTask ResumeAsync()
    {
        this._StatusCache = await this.InvokeJavaScriptAsync<SpeechSynthesisStatus>("resume");
    }

    private void HandleEvents(SpeechSynthesisUtterance utterancet)
    {
        utterancet.Start += this.OnStart;
        utterancet.Boundary += this.OnBoundary;
        utterancet.Mark += this.OnMark;
        utterancet.Pause += this.OnPause;
        utterancet.Resume += this.OnResume;
        utterancet.End += this.OnEnd;
        utterancet.Error += this.OnError;
    }

    private void OnStart(object? sender, SpeechSynthesisStatusEventArgs args) { this.UpdateStatus(args); UtteranceStarted?.Invoke(sender, args); }
    private void OnBoundary(object? sender, SpeechSynthesisStatusEventArgs args) { this.UpdateStatus(args); UtteranceReachedBoundary?.Invoke(sender, args); }
    private void OnMark(object? sender, SpeechSynthesisStatusEventArgs args) { this.UpdateStatus(args); UtteranceReachedMark?.Invoke(sender, args); }
    private void OnPause(object? sender, SpeechSynthesisStatusEventArgs args) { this.UpdateStatus(args); UtterancePaused?.Invoke(sender, args); }
    private void OnResume(object? sender, SpeechSynthesisStatusEventArgs args) { this.UpdateStatus(args); UtteranceResumed?.Invoke(sender, args); }
    private void OnEnd(object? sender, SpeechSynthesisStatusEventArgs args) { this.UpdateStatus(args); UtteranceEnded?.Invoke(sender, args); }
    private void OnError(object? sender, SpeechSynthesisStatusEventArgs args) { this.UpdateStatus(args); UtteranceError?.Invoke(sender, args); }

    private IJSObjectReference? _JSModule = null;

    private async ValueTask<T> InvokeJavaScriptAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)] T>(string identifier, params object[] args)
    {
        if (this._JSModule is null)
        {
            await this.Syncer.WaitAsync();
            try
            {
                if (this._JSModule is null)
                {
                    // Add version string for refresh token only navigator is online.
                    // (If the app runs on the offline mode, the module url with query parameters might cause the "resource not found" error.)
                    var isOnLine = await this.JSRuntime.InvokeAsync<bool>("Toolbelt.Blazor.getProperty", "navigator.onLine");

                    var scriptPath = "./_content/Toolbelt.Blazor.SpeechSynthesis/script.min.js";
                    if (isOnLine) scriptPath += $"?v={VersionInfo.VersionText}";

                    this._JSModule = await this.JSRuntime.InvokeAsync<IJSObjectReference>("import", scriptPath);
                }
            }
            finally { this.Syncer.Release(); }
        }

        return await this._JSModule.InvokeAsync<T>(identifier, args);
    }

    public async ValueTask DisposeAsync()
    {
        if (this._JSModule != null)
        {
            try { await this._JSModule.DisposeAsync(); }
            catch (JSDisconnectedException) { }
            catch { throw; }
        }
    }
}
