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

        private bool _Paused;

        private bool _Pending;

        private bool _Speaking;

        private List<SpeechSynthesisVoice> _Voices;

        public bool Paused => this.Refresh()._Paused;

        public bool Pending => this.Refresh()._Pending;

        public bool Speaking => this.Refresh()._Speaking;

        internal SpeechSynthesis()
        {
        }

        private SpeechSynthesis Refresh()
        {
            if ((LastRefreshTask?.IsCompleted ?? true) == true)
            {
                LastRefreshTask?.Dispose();
                if (_ObjectRefOfThis == null) _ObjectRefOfThis = new DotNetObjectRef(this);
                LastRefreshTask = JSRuntime.Current.InvokeAsync<object>(Namespace + ".refresh", _ObjectRefOfThis);
            }
            return this;
        }

        [JSInvokable(nameof(UpdateStatus)), EditorBrowsable(EditorBrowsableState.Never)]
        public void UpdateStatus(bool paused, bool pending, bool speaking)
        {
            this._Paused = paused;
            this._Pending = pending;
            this._Speaking = speaking;
        }

        public async Task<IReadOnlyCollection<SpeechSynthesisVoice>> GetVoicesAsync()
        {
            if (_Voices == null) _Voices = new List<SpeechSynthesisVoice>();
            var latestVoices = await JSRuntime.Current.InvokeAsync<SpeechSynthesisVoiceInternal[]>(Namespace + ".getVoices");
            var toAddVoices = latestVoices.Where(p1 => !_Voices.Any(p2 => p1.VoiceURI == p2.VoiceURI)).ToArray();
            var toRemoveVoices = _Voices.Where(p1 => !latestVoices.Any(p2 => p1.VoiceURI == p2.VoiceURI)).ToArray();

            _Voices.AddRange(toAddVoices.Select(v => new SpeechSynthesisVoice(v)));
            foreach (var voice in toRemoveVoices) _Voices.Remove(voice);

            return _Voices;
        }

        public void Speak(string text) => JSRuntime.Current.InvokeAsync<object>(Namespace + ".speak", text);

        public void Speak(SpeechSynthesisUtterance utterance) => JSRuntime.Current.InvokeAsync<object>(Namespace + ".speak", utterance);

        public void Cancel() => JSRuntime.Current.InvokeAsync<object>(Namespace + ".cancel");

        public void Pause() => JSRuntime.Current.InvokeAsync<object>(Namespace + ".pause");

        public void Resume() => JSRuntime.Current.InvokeAsync<object>(Namespace + ".resume");
    }
}
