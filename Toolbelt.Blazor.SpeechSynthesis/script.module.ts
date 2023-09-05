export namespace Toolbelt.Blazor.SpeechSynthesis {

    const undefined = 'undefined';
    const end = 'end';
    const error = 'error';
    const _cancel = 'cancel';

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
    const available = typeof speechSynthesis.getVoices !== undefined;

    const onVoicesChanged = (available && speechSynthesis.getVoices().length === 0) ?
        new Promise<void>(resolve => {
            if (typeof (speechSynthesis.addEventListener) === undefined) resolve();
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

    export const getStatus = () => ({ available, paused: speechSynthesis.paused, pending: speechSynthesis.pending, speaking: speechSynthesis.speaking });

    const getVoicesInternal = async () => {
        if (!available) return [];
        await onVoicesChanged;
        return speechSynthesis.getVoices();
    }

    export const getNumberOfVoices = async () => {
        const rawVoices = await getVoicesInternal();
        return rawVoices.length;
    }

    export const getVoices = async (args?: { begin: number, end: number }) => {
        const rawVoices = await getVoicesInternal();
        const voices = rawVoices.map(v => ({
            default: v.default,
            lang: v.lang,
            localService: v.localService,
            name: v.name,
            voiceURI: v.voiceURI
        }));
        return args ? voices.slice(args.begin, args.end) : voices;
    }

    export const speak = (utterance: SpeechSynthesisUtterance, dotnetUtterance: DotNetObjectRef): SpeechSynthesisStatus => {
        if (!available) return getStatus();
        if (utterance.voice !== null) {
            const voiceURI = utterance.voice.voiceURI;
            const lang = utterance.voice.lang;
            utterance.voice = speechSynthesis
                .getVoices()
                .find(v => v.voiceURI === voiceURI && v.lang.replace(/_/ig, '-') === lang)!;
        }
        utterance = Object.assign(new SpeechSynthesisUtterance(), utterance);

        const types = ["boundary", end, error, "mark", "pause", "resume", "start"];
        const f = function (ev: EventLikeObject): void {
            const stat = getStatus();
            dotnetUtterance.invokeMethodAsync('InvokeEvent', ev.type, stat);
            if (ev.type === end || ev.type === error || ev.type === _cancel) {
                types.forEach(t => utterance.removeEventListener(t, f));
                queue = queue.filter(q => q.f !== f);
            }
        }
        types.forEach(t => utterance.addEventListener(t, f));

        queue.push({ f });

        speechSynthesis.speak(utterance);

        setTimeout(() => {
            const cancellers = queue.filter(q => q.cancel === true);
            cancellers.forEach(c => c.f({ type: _cancel }));
        }, 0);
        return getStatus();
    }

    export const cancel = (): SpeechSynthesisStatus => {
        if (available) {
            speechSynthesis.cancel();
            queue.forEach(q => q.cancel = true);
        }
        return getStatus();
    }

    export const pause = (): SpeechSynthesisStatus => { if (available) speechSynthesis.pause(); return getStatus(); }

    export const resume = (): SpeechSynthesisStatus => { if (available) speechSynthesis.resume(); return getStatus(); }

    // The work around for iOS
    // - https://stackoverflow.com/a/62587365/1268000

    ((body: HTMLElement, clickEventName: 'click') => {
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