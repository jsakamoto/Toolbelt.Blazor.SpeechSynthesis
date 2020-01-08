namespace Toolbelt.Blazor.SpeechSynthesis {

    interface DotNetObjectRef {
        invokeMethodAsync(methodName: string, ...args: any[]): Promise<any>;
    }

    interface EventLikeObject {
        type: string;
    }

    const s = window.speechSynthesis || ({ paused: false, pending: false, speaking: false } as SpeechSynthesis);
    const available = typeof s.getVoices !== 'undefined';
    const onVoicesChanged = (available && s.getVoices().length === 0) ? new Promise<void>(resolve => s.addEventListener('voiceschanged', () => resolve())) : Promise.resolve();

    interface UtteranceQueue {
        f: (ev: EventLikeObject) => void;
        cancel?: boolean;
    }

    let queue = [] as UtteranceQueue[];

    export function refresh(objWrapper: DotNetObjectRef): void {
        objWrapper.invokeMethodAsync('UpdateStatus', available, s.paused, s.pending, s.speaking);
    }

    export function getVoices(): Promise<any[]> {
        if (!available) return Promise.resolve([] as any[]);
        return onVoicesChanged
            .then(() => s.getVoices().map(v => ({
                default: v.default,
                lang: v.lang,
                localService: v.localService,
                name: v.name,
                voiceURI: v.voiceURI
            })));
    }

    export function speak(sRef: DotNetObjectRef, arg: SpeechSynthesisUtterance, uRef: DotNetObjectRef): void {
        if (!available) return;
        const u = new SpeechSynthesisUtterance();
        if (arg.voice !== null) arg.voice = s.getVoices().find(v => v.voiceURI === arg.voice.voiceURI)!;
        Object.assign(u, arg);

        const types = ["boundary", "end", "error", "mark", "pause", "resume", "start"];
        const f = function (ev: EventLikeObject): void {
            refresh(sRef);
            uRef.invokeMethodAsync('InvokeEvent', ev.type);
            if (ev.type === 'end' || ev.type === 'error' || ev.type === 'cancel') {
                types.forEach(t => u.removeEventListener(t, f));
                queue = queue.filter(q => q.f !== f);
            }
        }
        types.forEach(t => u.addEventListener(t, f));

        queue.push({ f });

        s.speak(u);

        setTimeout(() => {
            const cancellers = queue.filter(q => q.cancel === true);
            cancellers.forEach(c => c.f({ type: 'cancel' }));
        }, 0);
    }

    export function cancel(): void {
        if (!available) return;
        s.cancel();
        queue.forEach(q => q.cancel = true);
    }

    export function pause(): void { if (available) s.pause(); }

    export function resume(): void { if (available) s.resume(); }
}