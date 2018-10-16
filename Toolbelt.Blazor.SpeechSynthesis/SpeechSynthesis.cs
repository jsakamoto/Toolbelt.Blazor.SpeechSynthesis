using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace Toolbelt.Blazor.SpeechSynthesis
{
    public class SpeechSynthesis
    {
        private static readonly string Namespace = "Toolbelt.Blazor.SpeechSynthesis";

        private Task LastRefreshTask = null;

        private DotNetObjectRef _ObjectRefOfThis;

        private bool _Available;

        private bool _Paused;

        private bool _Pending;

        private bool _Speaking;

        private List<SpeechSynthesisVoice> _Voices;

        public bool Available => this.Refresh()._Available;

        public bool Paused => this.Refresh()._Paused;

        public bool Pending => this.Refresh()._Pending;

        public bool Speaking => this.Refresh()._Speaking;

        internal SpeechSynthesis()
        {
        }

        private DotNetObjectRef GetObjectRef()
        {
            if (_ObjectRefOfThis == null) _ObjectRefOfThis = new DotNetObjectRef(this);
            return _ObjectRefOfThis;
        }

        internal SpeechSynthesis Refresh()
        {
            if ((LastRefreshTask?.IsCompleted ?? true) == true)
            {
                LastRefreshTask?.Dispose();
                LastRefreshTask = JSRuntime.Current.InvokeAsync<object>(Namespace + ".refresh", this.GetObjectRef());
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

        public async Task<IReadOnlyCollection<SpeechSynthesisVoice>> GetVoicesAsync()
        {
            if (_Voices == null) _Voices = new List<SpeechSynthesisVoice>();
            if (!Available) return _Voices;

            var latestVoices = await JSRuntime.Current.InvokeAsync<SpeechSynthesisVoiceInternal[]>(Namespace + ".getVoices");
            var toAddVoices = latestVoices.Where(p1 => !_Voices.Any(p2 => p1.VoiceURI == p2.VoiceURI)).ToArray();
            var toRemoveVoices = _Voices.Where(p1 => !latestVoices.Any(p2 => p1.VoiceURI == p2.VoiceURI)).ToArray();

            _Voices.AddRange(toAddVoices.Select(v => new SpeechSynthesisVoice(v)));
            foreach (var voice in toRemoveVoices) _Voices.Remove(voice);

            return _Voices;
        }

        public void Speak(string text) => this.Speak(new SpeechSynthesisUtterance { Text = text });

        public void Speak(SpeechSynthesisUtterance utterance)
        {
            if (Available) JSRuntime.Current.InvokeAsync<object>(Namespace + ".speak", this.GetObjectRef(), utterance, utterance.GetObjectRef());
        }

        public void Cancel() { if (Available) JSRuntime.Current.InvokeAsync<object>(Namespace + ".cancel"); }

        public void Pause() { if (Available) JSRuntime.Current.InvokeAsync<object>(Namespace + ".pause"); }

        public void Resume() { if (Available) JSRuntime.Current.InvokeAsync<object>(Namespace + ".resume"); }
    }
}
