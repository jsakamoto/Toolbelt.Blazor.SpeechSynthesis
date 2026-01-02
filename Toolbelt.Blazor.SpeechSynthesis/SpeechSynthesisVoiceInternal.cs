
namespace Toolbelt.Blazor.SpeechSynthesis;

internal class SpeechSynthesisVoiceInternal
{
    public string VoiceIdentity => this.VoiceURI + "|" + this.Lang;

    public bool Default { get; }

    public string? Lang { get; }

    public bool LocalService { get; }

    public string? Name { get; }

    public string? VoiceURI { get; }

    public SpeechSynthesisVoiceInternal(bool @default, string? lang, bool localService, string? name, string? voiceURI)
    {
        this.Default = @default;
        this.Lang = lang;
        this.LocalService = localService;
        this.Name = name;
        this.VoiceURI = voiceURI;
    }
}
