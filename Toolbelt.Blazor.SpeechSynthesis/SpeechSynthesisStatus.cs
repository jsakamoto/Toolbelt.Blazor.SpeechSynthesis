namespace Toolbelt.Blazor.SpeechSynthesis
{
    /// <summary>
    /// Represent a status of the Web Speech API SpeechSynthesis object.
    /// </summary>
    public class SpeechSynthesisStatus
    {
        /// <summary>
        /// Gets a value that indicates whether the Web Speech API is available or not.
        /// </summary>
        public bool Available { get; }

        /// <summary>
        /// Gets a value that indicates whether the SpeechSynthesis object is in a paused state or not.
        /// </summary>
        public bool Paused { get; }

        /// <summary>
        /// Gets a value that indicates whether the utterance queue contains as-yet-unspoken utterances or not.
        /// </summary>
        public bool Pending { get; }

        /// <summary>
        /// Gets a value that indicates whether an utterance is currently in the process of being spoken (includes SpeechSynthesis is in a paused state) or not.
        /// </summary>
        public bool Speaking { get; }

        /// <summary>
        /// Initialize a new instance of the SpeechSynthesisStatus class.
        /// </summary>
        /// <param name="available">A value that indicates whether the Web Speech API is available or not.</param>
        /// <param name="paused">A value that indicates whether the SpeechSynthesis object is in a paused state or not.</param>
        /// <param name="pending">A value that indicates whether the utterance queue contains as-yet-unspoken utterances or not.</param>
        /// <param name="speaking">A value that indicates whether an utterance is currently in the process of being spoken (includes SpeechSynthesis is in a paused state) or not.</param>
        public SpeechSynthesisStatus(bool available, bool paused, bool pending, bool speaking)
        {
            this.Available = available;
            this.Paused = paused;
            this.Pending = pending;
            this.Speaking = speaking;
        }
    }
}
