namespace Toolbelt.Blazor.SpeechSynthesis;

/// <summary>
/// Provides data for events that are raised when the status of the SpeechSynthesis object is changed.
/// </summary>
public class SpeechSynthesisStatusEventArgs : EventArgs
{
    /// <summary>
    /// Gets a status value of the Web Speech API SpeechSynthesis object.
    /// </summary>
    public SpeechSynthesisStatus Status { get; }

    /// <summary>
    /// Initialize a new instance of the SpeechSynthesisStatusEventArgs class.
    /// </summary>
    /// <param name="status">A status value of the Web Speech API SpeechSynthesis object.</param>
    public SpeechSynthesisStatusEventArgs(SpeechSynthesisStatus status)
    {
        this.Status = status;
    }
}
