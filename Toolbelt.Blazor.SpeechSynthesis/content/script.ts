namespace Toolbelt.Blazor.SpeechSynthesis {
    const s = window.speechSynthesis;
    const onVoicesChanged = s.getVoices().length == 0 ? new Promise<void>(resolve => s.addEventListener('voiceschanged', () => resolve())) : null;

    export function refresh(objWrapper: any): void {
        objWrapper.invokeMethodAsync('UpdateStatus', s.paused, s.pending, s.speaking);
    }

    export async function getVoices(): Promise<any[]> {
        if (onVoicesChanged != null) {
            await onVoicesChanged;
        }
        return s.getVoices().map(v => ({
            _Default: v.default,
            _Lang: v.lang,
            _LocalService: v.localService,
            _Name: v.name,
            _VoiceURI: v.voiceURI
        }));
    }

    export function speak(arg: string | SpeechSynthesisUtterance): void {
        const utterance = new SpeechSynthesisUtterance();
        if (typeof arg == 'string') utterance.text = arg;
        else {
            if (arg.voice != null) {
                arg.voice = s.getVoices().find(v => v.voiceURI == arg.voice.voiceURI);
            }
            Object.assign(utterance, arg);
        }
        s.speak(utterance);
    }

    export function cancel(): void { s.cancel(); }

    export function pause(): void { s.pause(); }

    export function resume(): void { s.resume(); }
}