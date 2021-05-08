using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace Toolbelt.Blazor.SpeechSynthesis
{
    /// <summary>
    /// The controller interface for the speech service of the Web Speech API.
    /// </summary>
    public class SpeechSynthesis
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
            if (_ObjectRefOfThis == null) _ObjectRefOfThis = DotNetObjectReference.Create(this);
            return _ObjectRefOfThis;
        }

        internal SpeechSynthesis Refresh()
        {
            if ((LastRefreshTask?.IsCompleted ?? true) == true)
            {
                LastRefreshTask?.Dispose();
                LastRefreshTask = InvokeJavaScriptAsync<object>("refresh", this.GetObjectRef()).AsTask();
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
            if (_Voices == null) _Voices = new List<SpeechSynthesisVoice>();

            var latestVoices = await InvokeJavaScriptAsync<SpeechSynthesisVoiceInternal[]>("getVoices");
            var toAddVoices = latestVoices.Where(p1 => !_Voices.Any(p2 => p1.VoiceIdentity == p2.VoiceIdentity)).ToArray();
            var toRemoveVoices = _Voices.Where(p1 => !latestVoices.Any(p2 => p1.VoiceIdentity == p2.VoiceIdentity)).ToArray();

            _Voices.AddRange(toAddVoices.Select(v => new SpeechSynthesisVoice(v)));
            foreach (var voice in toRemoveVoices) _Voices.Remove(voice);

            return _Voices;
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
            if (Available)
            {
                lock (_EventHandledUtterancesLock)
                {
                    var eventHandledUtterances = _EventHandledUtterances
                        .Select(wref => (WeakRef: wref, Utterance: wref.TryGetTarget(out var u) ? u : null))
                        .Where(item => item.Utterance != null)
                        .ToArray();
                    var eventHandled = eventHandledUtterances.Any(item => Object.ReferenceEquals(item.Utterance, utterance));

                    if (!eventHandled)
                    {
                        eventHandledUtterances = eventHandledUtterances
                            .ToArray();
                        _EventHandledUtterances = eventHandledUtterances.Select(item => item.WeakRef)
                            .Concat(new[] { new WeakReference<SpeechSynthesisUtterance>(utterance) })
                            .ToArray();
                        HandleEvents(utterance);
                    }
                    else
                        _EventHandledUtterances = eventHandledUtterances.Select(item => item.WeakRef).ToArray();
                }

                InvokeJavaScriptAsync<object>("speak", this.GetObjectRef(), utterance, utterance.GetObjectRef());
            }
        }

        /// <summary>
        /// Removes all utterances from the utterance queue.
        /// </summary>
        public void Cancel() { if (Available) InvokeJavaScriptAsync<object>("cancel"); }

        /// <summary>
        /// Puts the SpeechSynthesis object into a paused state.
        /// </summary>
        public void Pause() { if (Available) InvokeJavaScriptAsync<object>("pause"); }

        /// <summary>
        /// Puts the SpeechSynthesis object into a non-paused state if it was already paused.
        /// </summary>
        public void Resume() { if (Available) InvokeJavaScriptAsync<object>("resume"); }

#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

        private void HandleEvents(SpeechSynthesisUtterance utterancet)
        {
            utterancet.Start += OnStart;
            utterancet.Boundary += OnBoundary;
            utterancet.Mark += OnMark;
            utterancet.Pause += OnPause;
            utterancet.Resume += OnResume;
            utterancet.End += OnEnd;
            utterancet.Error += OnError;
        }

        private void OnStart(object sender, EventArgs args) { UtteranceStarted?.Invoke(sender, args); }
        private void OnBoundary(object sender, EventArgs args) { UtteranceReachedBoundary?.Invoke(sender, args); }
        private void OnMark(object sender, EventArgs args) { UtteranceReachedMark?.Invoke(sender, args); }
        private void OnPause(object sender, EventArgs args) { UtterancePaused?.Invoke(sender, args); }
        private void OnResume(object sender, EventArgs args) { UtteranceResumed?.Invoke(sender, args); }
        private void OnEnd(object sender, EventArgs args) { UtteranceEnded?.Invoke(sender, args); }
        private void OnError(object sender, EventArgs args) { UtteranceError?.Invoke(sender, args); }


        private async ValueTask<T> InvokeJavaScriptAsync<T>(string identifier, params object[] args)
        {
            if (!_JSLoaded && !this.Options.DisableClientScriptAutoInjection)
            {
                await Syncer.WaitAsync();
                try
                {
                    if (!_JSLoaded)
                    {
                        var version = this.GetType().Assembly.GetName().Version;
                        var scriptPath = "_content/Toolbelt.Blazor.SpeechSynthesis/script.min.js?v=" + version;
                        await JSRuntime.InvokeAsync<object>(
                            "eval",
                            "new Promise(r=>((d,t,s)=>(h=>h.querySelector(t+`[src=\"${s}\"]`)?r():(e=>(e.src=s,e.onload=r,h.appendChild(e)))(d.createElement(t)))(d.head))(document,'script','" + scriptPath + "'))");
                        _JSLoaded = true;
                    }
                }
                finally { Syncer.Release(); }
            }
            return await this.JSRuntime.InvokeAsync<T>(Prefix + identifier, args);
        }
    }
}
