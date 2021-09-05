using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace Toolbelt.Blazor.SpeechSynthesis
{
    /// <summary>
    /// The controller interface for the speech service of the Web Speech API.
    /// </summary>
    public class SpeechSynthesis
#if ENABLE_JSMODULE
        : IAsyncDisposable
#endif
    {
        private static readonly string Prefix = "Toolbelt.Blazor.SpeechSynthesis.";

        private SpeechSynthesisOptions Options;

        private readonly IJSRuntime JSRuntime;

        private readonly SemaphoreSlim Syncer = new SemaphoreSlim(1, 1);

        private bool _JSLoaded;

        private Task LastRefreshTask = null;

        private DotNetObjectReference<SpeechSynthesis> _ObjectRefOfThis;

        private bool _Available;

        private bool _Paused;

        private bool _Pending;

        private bool _Speaking;

        private List<SpeechSynthesisVoice> _Voices;

        private readonly object _EventHandledUtterancesLock = new object();

        private WeakReference<SpeechSynthesisUtterance>[] _EventHandledUtterances = new WeakReference<SpeechSynthesisUtterance>[0];

        /// <summary>
        /// Gets a value that indicates whether the Web Speech API is available or not.
        /// </summary>
        public bool Available => this.Refresh()._Available;

        /// <summary>
        /// Gets a value that indicates whether the SpeechSynthesis object is in a paused state or not.
        /// </summary>
        public bool Paused => this.Refresh()._Paused;

        /// <summary>
        /// Gets a value that indicates whether the utterance queue contains as-yet-unspoken utterances or not.
        /// </summary>
        public bool Pending => this.Refresh()._Pending;

        /// <summary>
        /// Gets a value that indicates whether an utterance is currently in the process of being spoken (includes SpeechSynthesis is in a paused state) or not.
        /// </summary>
        public bool Speaking => this.Refresh()._Speaking;

        /// <summary>
        /// Occurs when the utterance has begun to be spoken.
        /// </summary>
        public event EventHandler UtteranceStarted;

        /// <summary>
        /// Occurs when the spoken utterance reaches a word or sentence boundary.
        /// </summary>
        public event EventHandler UtteranceReachedBoundary;

        /// <summary>
        /// Occurs when the spoken utterance reaches a named SSML "mark" tag.
        /// </summary>
        public event EventHandler UtteranceReachedMark;

        /// <summary>
        /// Occurs when the utterance is paused part way through.
        /// </summary>
        public event EventHandler UtterancePaused;

        /// <summary>
        /// Occurs when a paused utterance is resumed.
        /// </summary>
        public event EventHandler UtteranceResumed;

        /// <summary>
        /// Occurs when the utterance has finished being spoken.
        /// </summary>
        public event EventHandler UtteranceEnded;

        /// <summary>
        /// Occurs when an error occurs that prevents the utterance from being succesfully spoken.
        /// </summary>
        public event EventHandler UtteranceError;

        internal SpeechSynthesis(IJSRuntime jSRuntime, SpeechSynthesisOptions options)
        {
            this.JSRuntime = jSRuntime;
            this.Options = options;
        }

        private DotNetObjectReference<SpeechSynthesis> GetObjectRef()
        {
            if (this._ObjectRefOfThis == null) this._ObjectRefOfThis = DotNetObjectReference.Create(this);
            return this._ObjectRefOfThis;
        }

        internal SpeechSynthesis Refresh()
        {
            if ((this.LastRefreshTask?.IsCompleted ?? true) == true)
            {
                this.LastRefreshTask?.Dispose();
                this.LastRefreshTask = this.InvokeJavaScriptAsync<object>("refresh", this.GetObjectRef()).AsTask();
            }
            return this;
        }

        [JSInvokable(nameof(UpdateStatus)), EditorBrowsable(EditorBrowsableState.Never)]
        public void UpdateStatus(bool available, bool paused, bool pending, bool speaking)
        {
            this._Available = available;
            this._Paused = paused;
            this._Pending = pending;
            this._Speaking = speaking;
        }

        /// <summary>
        /// Returns a collection of SpeechSynthesisVoice objects representing all the available voices on the current device.
        /// </summary>
        public async Task<IReadOnlyCollection<SpeechSynthesisVoice>> GetVoicesAsync()
        {
            if (this._Voices == null) this._Voices = new List<SpeechSynthesisVoice>();

            var latestVoices = await this.InvokeJavaScriptAsync<SpeechSynthesisVoiceInternal[]>("getVoices");
            var toAddVoices = latestVoices.Where(p1 => !this._Voices.Any(p2 => p1.VoiceIdentity == p2.VoiceIdentity)).ToArray();
            var toRemoveVoices = this._Voices.Where(p1 => !latestVoices.Any(p2 => p1.VoiceIdentity == p2.VoiceIdentity)).ToArray();

            this._Voices.AddRange(toAddVoices.Select(v => new SpeechSynthesisVoice(v)));
            foreach (var voice in toRemoveVoices) this._Voices.Remove(voice);

            return this._Voices;
        }

        /// <summary>
        /// Adds an utterance initialized with specified text to the utterance queue.
        /// <para>it will be spoken when any other utterances queued before it have been spoken.</para>
        /// </summary>
        public void Speak(string text) => this.Speak(new SpeechSynthesisUtterance { Text = text });

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

        /// <summary>
        /// Adds an utterance to the utterance queue.
        /// <para>it will be spoken when any other utterances queued before it have been spoken.</para>
        /// </summary>
        public void Speak(SpeechSynthesisUtterance utterance)
        {
            if (this.Available)
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

                this.InvokeJavaScriptAsync<object>("speak", this.GetObjectRef(), utterance, utterance.GetObjectRef());
            }
        }

        /// <summary>
        /// Removes all utterances from the utterance queue.
        /// </summary>
        public void Cancel() { if (this.Available) this.InvokeJavaScriptAsync<object>("cancel"); }

        /// <summary>
        /// Puts the SpeechSynthesis object into a paused state.
        /// </summary>
        public void Pause() { if (this.Available) this.InvokeJavaScriptAsync<object>("pause"); }

        /// <summary>
        /// Puts the SpeechSynthesis object into a non-paused state if it was already paused.
        /// </summary>
        public void Resume() { if (this.Available) this.InvokeJavaScriptAsync<object>("resume"); }

#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

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

        private void OnStart(object sender, EventArgs args) { UtteranceStarted?.Invoke(sender, args); }
        private void OnBoundary(object sender, EventArgs args) { UtteranceReachedBoundary?.Invoke(sender, args); }
        private void OnMark(object sender, EventArgs args) { UtteranceReachedMark?.Invoke(sender, args); }
        private void OnPause(object sender, EventArgs args) { UtterancePaused?.Invoke(sender, args); }
        private void OnResume(object sender, EventArgs args) { UtteranceResumed?.Invoke(sender, args); }
        private void OnEnd(object sender, EventArgs args) { UtteranceEnded?.Invoke(sender, args); }
        private void OnError(object sender, EventArgs args) { UtteranceError?.Invoke(sender, args); }

        private string GetVersionText()
        {
            var assembly = this.GetType().Assembly;
            var version = assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                .InformationalVersion ?? assembly.GetName().Version.ToString();
            return version;
        }

#if ENABLE_JSMODULE

        private IJSObjectReference _JSModule = null;

        private async ValueTask<T> InvokeJavaScriptAsync<T>(string identifier, params object[] args)
        {
            if (!this._JSLoaded)
            {
                await this.Syncer.WaitAsync();
                try
                {
                    if (!this._JSLoaded)
                    {
                        if (!this.Options.DisableClientScriptAutoInjection)
                        {
                            var version = this.GetVersionText();
                            var scriptPath = $"./_content/Toolbelt.Blazor.SpeechSynthesis/script.module.min.js?v={version}";
                            this._JSModule = await this.JSRuntime.InvokeAsync<IJSObjectReference>("import", scriptPath);
                        }
                        else
                        {
                            try { await this.JSRuntime.InvokeVoidAsync("eval", "Toolbelt.Blazor.SpeechSynthesis.ready"); } catch { }
                        }
                        this._JSLoaded = true;
                    }
                }
                finally { this.Syncer.Release(); }
            }

            if (this._JSModule != null)
                return await this._JSModule.InvokeAsync<T>(Prefix + identifier, args);
            else
                return await this.JSRuntime.InvokeAsync<T>(Prefix + identifier, args);
        }

        public async ValueTask DisposeAsync()
        {
            if (_JSModule != null)
            {
                await _JSModule.DisposeAsync();
            }
        }
#else
        private async ValueTask<T> InvokeJavaScriptAsync<T>(string identifier, params object[] args)
        {
            if (!this._JSLoaded)
            {
                await this.Syncer.WaitAsync();
                try
                {
                    if (!this._JSLoaded)
                    {
                        if (!this.Options.DisableClientScriptAutoInjection)
                        {
                            var version = this.GetVersionText();
                            var scriptPath = "_content/Toolbelt.Blazor.SpeechSynthesis/script.min.js?v=" + version;
                            await this.JSRuntime.InvokeVoidAsync("eval", "new Promise(r=>((d,t,s)=>(h=>h.querySelector(t+`[src^=\"${s}\"]`)?r():(e=>(e.src=s,e.onload=r,h.appendChild(e)))(d.createElement(t)))(d.head))(document,'script','" + scriptPath + "'))");
                        }
                        try { await this.JSRuntime.InvokeVoidAsync("eval", "Toolbelt.Blazor.SpeechSynthesis.ready"); } catch { }
                        this._JSLoaded = true;
                    }
                }
                finally { this.Syncer.Release(); }
            }

            return await this.JSRuntime.InvokeAsync<T>(Prefix + identifier, args);
        }
#endif
    }
}
