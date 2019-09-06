using System;
using System.ComponentModel;
using Microsoft.JSInterop;

namespace Toolbelt.Blazor.SpeechSynthesis
{
    /// <summary>
    /// Represents a speech request.
    /// </summary>
    public class SpeechSynthesisUtterance
    {
        /// <summary>
        /// Gets or sets the BCP 47 language tag of the utterance.
        /// <para>If unset (empty string), the app's (i.e. the &lt;html&gt; lang value) lang will be used, or the user-agent default if that is unset too.</para>
        /// </summary>
        public string Lang { get; set; } = "";

        /// <summary>
        /// Gets or sets the pitch at which the utterance will be spoken at.
        /// <para>It can range between 0.0 (lowest) and 2.0 (highest), with 1.0 being the default pitch for the current platform or voice.</para>
        /// </summary>
        public double Pitch { get; set; } = 1.0;

        /// <summary>
        /// Gets or sets the speed at which the utterance will be spoken at.
        /// <para>It can range between 0.1 (lowest) and 10.0 (highest), with 1.0 being the default rate for the current platform or voice, which should correspond to a normal speaking rate.</para>
        /// </summary>
        public double Rate { get; set; } = 1.0;

        /// <summary>
        /// Gets or sets the text that will be synthesised when the utterance is spoken.
        /// <para>The text may be provided as plain text, or a well-formed SSML document.</para>
        /// </summary>
        public string Text { get; set; } = "";

        /// <summary>
        /// Gets or sets the volume that the utterance will be spoken at.
        /// <para>It can range between 0.0 (lowest) and 1.0 (highest), with 1.0 being the default volume for the current platform or voice.</para>
        /// </summary>
        public double Volume { get; set; } = 1;

        /// <summary>
        /// Gets or sets the voice that will be used to speak the utterance.
        /// <para>This should be set to one of the SpeechSynthesisVoice objects returned by SpeechSynthesis.GetVoicesAsync().</para>
        /// <para>If not set (it will be null) by the time the utterance is spoken, the voice used will be the most suitable default voice available for the utterance's lang setting.</para>
        /// </summary>
        public SpeechSynthesisVoice Voice { get; set; }

        /// <summary>
        /// Occurs when the utterance has begun to be spoken.
        /// </summary>
        public event EventHandler Start;

        /// <summary>
        /// Occurs when the spoken utterance reaches a word or sentence boundary.
        /// </summary>
        public event EventHandler Boundary;

        /// <summary>
        /// Occurs when the spoken utterance reaches a named SSML "mark" tag.
        /// </summary>
        public event EventHandler Mark;

        /// <summary>
        /// Occurs when the utterance is paused part way through.
        /// </summary>
        public event EventHandler Pause;

        /// <summary>
        /// Occurs when a paused utterance is resumed.
        /// </summary>
        public event EventHandler Resume;

        /// <summary>
        /// Occurs when the utterance has finished being spoken.
        /// </summary>
        public event EventHandler End;

        /// <summary>
        /// Occurs when an error occurs that prevents the utterance from being succesfully spoken.
        /// </summary>
        public event EventHandler Error;

        private DotNetObjectReference<SpeechSynthesisUtterance> _ObjectRef;

        private int _ObjectRefCounter = 0;

        internal DotNetObjectReference<SpeechSynthesisUtterance> GetObjectRef()
        {
            _ObjectRefCounter++;
            if (_ObjectRefCounter == 1) _ObjectRef = DotNetObjectReference.Create(this);
            return _ObjectRef;
        }

        private void ReleaseObjectRef()
        {
            _ObjectRefCounter--;
            if (_ObjectRefCounter == 0)
            {
                _ObjectRef.Dispose();
                _ObjectRef = null;
            }
        }

        [JSInvokable(nameof(InvokeEvent)), EditorBrowsable(EditorBrowsableState.Never)]
        public void InvokeEvent(string type)
        {
            switch (type)
            {
                case "start":
                    Start?.Invoke(this, EventArgs.Empty);
                    break;
                case "boundary":
                    Boundary?.Invoke(this, EventArgs.Empty);
                    break;
                case "mark":
                    Mark?.Invoke(this, EventArgs.Empty);
                    break;
                case "pause":
                    Pause?.Invoke(this, EventArgs.Empty);
                    break;
                case "resume":
                    Resume?.Invoke(this, EventArgs.Empty);
                    break;
                case "end":
                    End?.Invoke(this, EventArgs.Empty);
                    ReleaseObjectRef();
                    break;
                case "error":
                    Error?.Invoke(this, EventArgs.Empty);
                    ReleaseObjectRef();
                    break;
                case "cancel":
                    ReleaseObjectRef();
                    break;
                default:
                    break;
            }
        }
    }
}
