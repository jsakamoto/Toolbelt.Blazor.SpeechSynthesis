using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace Toolbelt.Blazor.SpeechSynthesis
{
    /// <summary>
    /// The controller interface for the speech service of the Web Speech API.
    /// </summary>
    public class SpeechSynthesis
    {
        private static readonly string Namespace = "Toolbelt.Blazor.SpeechSynthesis";

        private readonly IJSRuntime JSRuntime;

        private Task LastRefreshTask = null;

        private DotNetObjectRef<SpeechSynthesis> _ObjectRefOfThis;

        private bool _Available;

        private bool _Paused;

        private bool _Pending;

        private bool _Speaking;

        private List<SpeechSynthesisVoice> _Voices;

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

        internal SpeechSynthesis(IJSRuntime jSRuntime)
        {
            this.JSRuntime = jSRuntime;
        }

        private DotNetObjectRef<SpeechSynthesis> GetObjectRef()
        {
            if (_ObjectRefOfThis == null) _ObjectRefOfThis = DotNetObjectRef.Create(this);
            return _ObjectRefOfThis;
        }

        internal SpeechSynthesis Refresh()
        {
            if ((LastRefreshTask?.IsCompleted ?? true) == true)
            {
                LastRefreshTask?.Dispose();
                LastRefreshTask = JSRuntime.InvokeAsync<object>(Namespace + ".refresh", this.GetObjectRef());
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

            var latestVoices = await JSRuntime.InvokeAsync<SpeechSynthesisVoiceInternal[]>(Namespace + ".getVoices");
            var toAddVoices = latestVoices.Where(p1 => !_Voices.Any(p2 => p1.VoiceURI == p2.VoiceURI)).ToArray();
            var toRemoveVoices = _Voices.Where(p1 => !latestVoices.Any(p2 => p1.VoiceURI == p2.VoiceURI)).ToArray();

            _Voices.AddRange(toAddVoices.Select(v => new SpeechSynthesisVoice(v)));
            foreach (var voice in toRemoveVoices) _Voices.Remove(voice);

            return _Voices;
        }

        /// <summary>
        /// Adds an utterance initialized with specified text to the utterance queue.
        /// <para>it will be spoken when any other utterances queued before it have been spoken.</para>
        /// </summary>
        public void Speak(string text) => this.Speak(new SpeechSynthesisUtterance { Text = text });

        /// <summary>
        /// Adds an utterance to the utterance queue.
        /// <para>it will be spoken when any other utterances queued before it have been spoken.</para>
        /// </summary>
        public void Speak(SpeechSynthesisUtterance utterance)
        {
            if (Available) JSRuntime.InvokeAsync<object>(Namespace + ".speak", this.GetObjectRef(), utterance, utterance.GetObjectRef());
        }

        /// <summary>
        /// Removes all utterances from the utterance queue.
        /// </summary>
        public void Cancel() { if (Available) JSRuntime.InvokeAsync<object>(Namespace + ".cancel"); }

        /// <summary>
        /// Puts the SpeechSynthesis object into a paused state.
        /// </summary>
        public void Pause() { if (Available) JSRuntime.InvokeAsync<object>(Namespace + ".pause"); }

        /// <summary>
        /// Puts the SpeechSynthesis object into a non-paused state if it was already paused.
        /// </summary>
        public void Resume() { if (Available) JSRuntime.InvokeAsync<object>(Namespace + ".resume"); }
    }
}
