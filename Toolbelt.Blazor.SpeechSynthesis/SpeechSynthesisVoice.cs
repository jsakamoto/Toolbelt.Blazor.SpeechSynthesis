
namespace Toolbelt.Blazor.SpeechSynthesis
{
    public class SpeechSynthesisVoice
    {
        public bool Default { get; }

        public string Lang { get; }

        public bool LocalService { get; }

        public string Name { get; }

        public string VoiceURI { get; }

        internal SpeechSynthesisVoice(SpeechSynthesisVoiceInternal voice)
        {
            this.Default = voice.Default;
            this.Lang = voice.Lang;
            this.LocalService = voice.LocalService;
            this.Name = voice.Name;
            this.VoiceURI = voice.VoiceURI;
        }
    }
}
