namespace Toolbelt.Blazor.SpeechSynthesis {
    const s = window.speechSynthesis || ({ paused: false, pending: false, speaking: false } as SpeechSynthesis);
    const available = (typeof s.getVoices) != 'undefined';
    const onVoicesChanged = (available && s.getVoices().length == 0) ? new Promise<void>(resolve => s.addEventListener('voiceschanged', () => resolve())) : null;

    export function refresh(objWrapper: any): void {
        objWrapper.invokeMethodAsync('UpdateStatus', available, s.paused, s.pending, s.speaking);
    }

    export async function getVoices(): Promise<any[]> {
        if (onVoicesChanged != null) {
            await onVoicesChanged;
        }
        return s.getVoices().map(v => ({
            default: v.default,
            lang: v.lang,
            localService: v.localService,
            name: v.name,
            voiceURI: v.voiceURI
        }));
    }

    export function speak(sRef: any, arg: SpeechSynthesisUtterance, uRef: any): void {
        const u = new SpeechSynthesisUtterance();
        if (arg.voice != null) arg.voice = s.getVoices().find(v => v.voiceURI == arg.voice.voiceURI);
        Object.assign(u, arg);

        const types = ["boundary", "end", "error", "mark", "pause", "resume", "start"];
        const f = function (ev: Event) {
            refresh(sRef);
            uRef.invokeMethodAsync('InvokeEvent', ev.type);
            if (ev.type == 'end' || ev.type == 'error') types.forEach(t => u.removeEventListener(t, f));
        }
        types.forEach(t => u.addEventListener(t, f));

        s.speak(u);
    }

    export function cancel(): void { s.cancel(); }

    export function pause(): void { s.pause(); }

    export function resume(): void { s.resume(); }
}