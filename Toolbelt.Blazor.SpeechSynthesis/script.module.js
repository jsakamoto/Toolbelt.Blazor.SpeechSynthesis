export var Toolbelt;
(function (Toolbelt) {
    var Blazor;
    (function (Blazor) {
        var SpeechSynthesis;
        (function (SpeechSynthesis) {
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
            SpeechSynthesis.getStatus = () => ({ available, paused: speechSynthesis.paused, pending: speechSynthesis.pending, speaking: speechSynthesis.speaking });
            const getVoicesInternal = async () => {
                if (!available)
                    return [];
                await onVoicesChanged;
                return speechSynthesis.getVoices();
            };
            SpeechSynthesis.getNumberOfVoices = async () => {
                const rawVoices = await getVoicesInternal();
                return rawVoices.length;
            };
            SpeechSynthesis.getVoices = async (args) => {
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
            SpeechSynthesis.speak = (utterance, dotnetUtterance) => {
                if (!available)
                    return SpeechSynthesis.getStatus();
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
                    const stat = SpeechSynthesis.getStatus();
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
                return SpeechSynthesis.getStatus();
            };
            SpeechSynthesis.cancel = () => {
                if (available) {
                    speechSynthesis.cancel();
                    queue.forEach(q => q.cancel = true);
                }
                return SpeechSynthesis.getStatus();
            };
            SpeechSynthesis.pause = () => { if (available)
                speechSynthesis.pause(); return SpeechSynthesis.getStatus(); };
            SpeechSynthesis.resume = () => { if (available)
                speechSynthesis.resume(); return SpeechSynthesis.getStatus(); };
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
        })(SpeechSynthesis = Blazor.SpeechSynthesis || (Blazor.SpeechSynthesis = {}));
    })(Blazor = Toolbelt.Blazor || (Toolbelt.Blazor = {}));
})(Toolbelt || (Toolbelt = {}));
