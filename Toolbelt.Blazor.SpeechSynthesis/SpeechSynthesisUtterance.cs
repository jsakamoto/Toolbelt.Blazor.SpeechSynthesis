using System;
using System.ComponentModel;
using Microsoft.JSInterop;

namespace Toolbelt.Blazor.SpeechSynthesis
{
    public class SpeechSynthesisUtterance
    {
        public string Lang { get; set; } = "";

        public double Pitch { get; set; } = 1.0;

        public double Rate { get; set; } = 1.0;

        public string Text { get; set; } = "";

        public double Volume { get; set; } = 1;

        public SpeechSynthesisVoice Voice { get; set; }

        public event EventHandler Start;

        public event EventHandler Boundary;

        public event EventHandler Mark;

        public event EventHandler Pause;

        public event EventHandler Resume;

        public event EventHandler End;

        public event EventHandler Error;

        private DotNetObjectRef _ObjectRef;

        internal DotNetObjectRef GetObjectRef()
        {
            if (_ObjectRef == null) _ObjectRef = new DotNetObjectRef(this);
            return _ObjectRef;
        }

        private void ReleaseObjectRef()
        {
            _ObjectRef.Dispose();
            _ObjectRef = null;
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
                default:
                    break;
            }
        }
    }
}
