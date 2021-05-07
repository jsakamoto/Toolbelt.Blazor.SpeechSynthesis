namespace Toolbelt.Blazor.SpeechSynthesis {

    interface DotNetObjectRef {
        invokeMethodAsync(methodName: string, ...args: any[]): Promise<any>;
    }

    interface EventLikeObject {
        type: string;
    }

    const speechSynthesis = window.speechSynthesis || ({ paused: false, pending: false, speaking: false } as SpeechSynthesis);
    const available = typeof speechSynthesis.getVoices !== 'undefined';
    const onVoicesChanged = (available && speechSynthesis.getVoices().length === 0) ? new Promise<void>(resolve => speechSynthesis.addEventListener('voiceschanged', () => resolve())) : Promise.resolve();

    interface UtteranceQueue {
        f: (ev: EventLikeObject) => void;
        cancel?: boolean;
    }

    let queue = [] as UtteranceQueue[];

    export function refresh(dotnetSpeechSynthesis: DotNetObjectRef): void {
        dotnetSpeechSynthesis.invokeMethodAsync('UpdateStatus', available, speechSynthesis.paused, speechSynthesis.pending, speechSynthesis.speaking);
    }

    export function getVoices(): Promise<any[]> {
        if (!available) return Promise.resolve([] as any[]);
        return onVoicesChanged
            .then(() => speechSynthesis.getVoices().map(v => ({
                default: v.default,
                lang: v.lang,
                localService: v.localService,
                name: v.name,
                voiceURI: v.voiceURI
            })));
    }

    export function speak(
        dotnetSpeechSynthesis: DotNetObjectRef,
        utterance: SpeechSynthesisUtterance,
        dotnetUtterance: DotNetObjectRef
    ): void {
        if (!available) return;
        if (utterance.voice !== null) {
            const voiceURI = utterance.voice.voiceURI;
            const lang = utterance.voice.lang;
            utterance.voice = speechSynthesis
                .getVoices()
                .find(v => v.voiceURI === voiceURI && v.lang.replace(/_/ig, '-') === lang)!;
        }
        utterance = Object.assign(new SpeechSynthesisUtterance(), utterance);

        const types = ["boundary", "end", "error", "mark", "pause", "resume", "start"];
        const f = function (ev: EventLikeObject): void {
            refresh(dotnetSpeechSynthesis);
            dotnetUtterance.invokeMethodAsync('InvokeEvent', ev.type);
            if (ev.type === 'end' || ev.type === 'error' || ev.type === 'cancel') {
                types.forEach(t => utterance.removeEventListener(t, f));
                queue = queue.filter(q => q.f !== f);
            }
        }
        types.forEach(t => utterance.addEventListener(t, f));

        queue.push({ f });

        speechSynthesis.speak(utterance);

        setTimeout(() => {
            const cancellers = queue.filter(q => q.cancel === true);
            cancellers.forEach(c => c.f({ type: 'cancel' }));
        }, 0);
    }

    export function cancel(): void {
        if (!available) return;
        speechSynthesis.cancel();
        queue.forEach(q => q.cancel = true);
    }

    export function pause(): void { if (available) speechSynthesis.pause(); }

    export function resume(): void { if (available) speechSynthesis.resume(); }
}