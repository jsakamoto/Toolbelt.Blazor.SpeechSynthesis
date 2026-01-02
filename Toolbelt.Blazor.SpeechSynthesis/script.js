const undefined = 'undefined';
const end = 'end';
const error = 'error';
const _cancel = 'cancel';
const speechSynthesis = window.speechSynthesis || { paused: false, pending: false, speaking: false };
const available = typeof speechSynthesis.getVoices !== undefined;
const onVoicesChanged = (available && speechSynthesis.getVoices().length === 0) ?
    new Promise(resolve => {
        if (typeof (speechSynthesis.addEventListener) === undefined)
            resolve();
        speechSynthesis.addEventListener('voiceschanged', () => {
            if (speechSynthesis.getVoices().length > 0)
                resolve();
        });
    }) :
    Promise.resolve();
let queue = [];
export const getStatus = () => ({ available, paused: speechSynthesis.paused, pending: speechSynthesis.pending, speaking: speechSynthesis.speaking });
const getVoicesInternal = async () => {
    if (!available)
        return [];
    await onVoicesChanged;
    return speechSynthesis.getVoices();
};
export const getNumberOfVoices = async () => {
    const rawVoices = await getVoicesInternal();
    return rawVoices.length;
};
export const getVoices = async (args) => {
    const rawVoices = await getVoicesInternal();
    const voices = rawVoices.map(v => ({
        default: v.default,
        lang: v.lang,
        localService: v.localService,
        name: v.name,
        voiceURI: v.voiceURI
    }));
    return args ? voices.slice(args.begin, args.end) : voices;
};
export const speak = (utterance, dotnetUtterance) => {
    if (!available)
        return getStatus();
    if (utterance.voice !== null) {
        const voiceURI = utterance.voice.voiceURI;
        const lang = utterance.voice.lang;
        utterance.voice = speechSynthesis
            .getVoices()
            .find(v => v.voiceURI === voiceURI && v.lang.replace(/_/ig, '-') === lang);
    }
    utterance = Object.assign(new SpeechSynthesisUtterance(), utterance);
    const types = ["boundary", end, error, "mark", "pause", "resume", "start"];
    const f = function (ev) {
        const stat = getStatus();
        dotnetUtterance.invokeMethodAsync('InvokeEvent', ev.type, stat);
        if (ev.type === end || ev.type === error || ev.type === _cancel) {
            types.forEach(t => utterance.removeEventListener(t, f));
            queue = queue.filter(q => q.f !== f);
        }
    };
    types.forEach(t => utterance.addEventListener(t, f));
    queue.push({ f });
    speechSynthesis.speak(utterance);
    setTimeout(() => {
        const cancellers = queue.filter(q => q.cancel === true);
        cancellers.forEach(c => c.f({ type: _cancel }));
    }, 0);
    return getStatus();
};
export const cancel = () => {
    if (available) {
        speechSynthesis.cancel();
        queue.forEach(q => q.cancel = true);
    }
    return getStatus();
};
export const pause = () => { if (available)
    speechSynthesis.pause(); return getStatus(); };
export const resume = () => { if (available)
    speechSynthesis.resume(); return getStatus(); };
((body, clickEventName) => {
    function f() {
        try {
            const u = new SpeechSynthesisUtterance('');
            u.volume = 0;
            speechSynthesis.speak(u);
        }
        catch (e) {
            console.error(e);
        }
        body.removeEventListener(clickEventName, f);
    }
    ;
    body.addEventListener(clickEventName, f);
})(document.body, 'click');
