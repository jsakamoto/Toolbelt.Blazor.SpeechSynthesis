export namespace Toolbelt.Blazor.SpeechSynthesis {

    interface DotNetObjectRef {
        invokeMethodAsync(methodName: string, ...args: any[]): Promise<any>;
    }

    interface EventLikeObject {
        type: string;
    }

    interface SpeechSynthesisStatus {
        available: boolean;
        paused: boolean;
        pending: boolean;
        speaking: boolean;
    }

    const speechSynthesis = window.speechSynthesis || ({ paused: false, pending: false, speaking: false } as SpeechSynthesis);
    const available = typeof speechSynthesis.getVoices !== 'undefined';

    const onVoicesChanged = (available && speechSynthesis.getVoices().length === 0) ?
        new Promise<void>(resolve => {
            if (typeof (speechSynthesis.addEventListener) === 'undefined') resolve();
            speechSynthesis.addEventListener('voiceschanged', () => {
                if (speechSynthesis.getVoices().length > 0) resolve();
            })
        }) :
        Promise.resolve();

    interface UtteranceQueue {
        f: (ev: EventLikeObject) => void;
        cancel?: boolean;
    }

    let queue = [] as UtteranceQueue[];

    export function getStatus(): SpeechSynthesisStatus {
        return { available, paused: speechSynthesis.paused, pending: speechSynthesis.pending, speaking: speechSynthesis.speaking };
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
        utterance: SpeechSynthesisUtterance,
        dotnetUtterance: DotNetObjectRef
    ): SpeechSynthesisStatus {
        if (!available) return getStatus();
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
            const stat = getStatus();
            dotnetUtterance.invokeMethodAsync('InvokeEvent', ev.type, stat);
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
        return getStatus();
    }

    export function cancel(): SpeechSynthesisStatus {
        if (available) {
            speechSynthesis.cancel();
            queue.forEach(q => q.cancel = true);
        }
        return getStatus();
    }

    export function pause(): SpeechSynthesisStatus { if (available) speechSynthesis.pause(); return getStatus(); }

    export function resume(): SpeechSynthesisStatus { if (available) speechSynthesis.resume(); return getStatus(); }

    // The work around for iOS
    // - https://stackoverflow.com/a/62587365/1268000

    (function (body: HTMLElement, clickEventName: 'click') {
        function f() {
            try {
                const u = new SpeechSynthesisUtterance('');
                u.volume = 0;
                speechSynthesis.speak(u);
            }
            catch (e) { console.error(e); }
            body.removeEventListener(clickEventName, f);
        };
        body.addEventListener(clickEventName, f);
    })(document.body, 'click');
}