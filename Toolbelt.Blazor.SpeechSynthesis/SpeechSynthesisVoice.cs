using System.ComponentModel;

namespace Toolbelt.Blazor.SpeechSynthesis
{
    public class SpeechSynthesisVoice
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool _Default;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public string _Lang;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool _LocalService;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public string _Name;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public string _VoiceURI;

        public bool Default => _Default;

        public string Lang => _Lang;

        public bool LocalService => _LocalService;

        public string Name => _Name;

        public string VoiceURI => _VoiceURI;
    }
}
