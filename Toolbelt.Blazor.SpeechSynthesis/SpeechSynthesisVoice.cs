
namespace Toolbelt.Blazor.SpeechSynthesis
{
    /// <summary>
    /// Represents a voice that the system supports.
    /// </summary>
    public class SpeechSynthesisVoice
    {
        /// <summary>
        /// Gets a value that indicates whether the voice is the default voice for the current app language or not.
        /// </summary>
        public bool Default { get; }

        /// <summary>
        /// Gets a BCP 47 language tag indicating the language of the voice.
        /// </summary>
        public string Lang { get; }

        /// <summary>
        /// Gets a value that indicates whether the voice is supplied by a local speech synthesizer service or not (a remote speech synthesizer service).
        /// </summary>
        public bool LocalService { get; }

        /// <summary>
        /// Gets a human-readable name that represents the voice.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the type of URI and location of the speech synthesis service for this voice.
        /// </summary>
        public string VoiceURI { get; }

        internal SpeechSynthesisVoice(SpeechSynthesisVoiceInternal voice)
        {
            this.Default = voice.Default;
            this.Lang = voice.Lang.Replace('_', '-');
            this.LocalService = voice.LocalService;
            this.Name = voice.Name;
            this.VoiceURI = voice.VoiceURI;
        }
    }
}
