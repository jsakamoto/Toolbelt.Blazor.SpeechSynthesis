
namespace Toolbelt.Blazor.SpeechSynthesis
{
    internal class SpeechSynthesisVoiceInternal
    {
        public string VoiceIdentity => this.VoiceURI + "|" + this.Lang;

        public bool Default { get; set; }

        public string? Lang { get; set; }

        public bool LocalService { get; set; }

        public string? Name { get; set; }

        public string? VoiceURI { get; set; }
    }
}
