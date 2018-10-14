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
    }
}
