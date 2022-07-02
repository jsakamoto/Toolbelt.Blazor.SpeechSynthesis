export var Toolbelt;
(function (Toolbelt) {
    var Blazor;
    (function (Blazor) {
        var SpeechSynthesis;
        (function (SpeechSynthesis) {
            const speechSynthesis = window.speechSynthesis || { paused: false, pending: false, speaking: false };
            const available = typeof speechSynthesis.getVoices !== 'undefined';
            const onVoicesChanged = (available && speechSynthesis.getVoices().length === 0) ?
                new Promise(resolve => {
                    if (typeof (speechSynthesis.addEventListener) === 'undefined')
                        resolve();
                    speechSynthesis.addEventListener('voiceschanged', () => {
                        if (speechSynthesis.getVoices().length > 0)
                            resolve();
                    });
                }) :
                Promise.resolve();
            let queue = [];
            function getStatus() {
                return { available, paused: speechSynthesis.paused, pending: speechSynthesis.pending, speaking: speechSynthesis.speaking };
            }
            SpeechSynthesis.getStatus = getStatus;
            function getVoices() {
                if (!available)
                    return Promise.resolve([]);
                return onVoicesChanged
                    .then(() => speechSynthesis.getVoices().map(v => ({
                    default: v.default,
                    lang: v.lang,
                    localService: v.localService,
                    name: v.name,
                    voiceURI: v.voiceURI
                })));
            }
            SpeechSynthesis.getVoices = getVoices;
            function speak(utterance, dotnetUtterance) {
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
                const types = ["boundary", "end", "error", "mark", "pause", "resume", "start"];
                const f = function (ev) {
                    const stat = getStatus();
                    dotnetUtterance.invokeMethodAsync('InvokeEvent', ev.type, stat);
                    if (ev.type === 'end' || ev.type === 'error' || ev.type === 'cancel') {
                        types.forEach(t => utterance.removeEventListener(t, f));
                        queue = queue.filter(q => q.f !== f);
                    }
                };
                types.forEach(t => utterance.addEventListener(t, f));
                queue.push({ f });
                speechSynthesis.speak(utterance);
                setTimeout(() => {
                    const cancellers = queue.filter(q => q.cancel === true);
                    cancellers.forEach(c => c.f({ type: 'cancel' }));
                }, 0);
                return getStatus();
            }
            SpeechSynthesis.speak = speak;
            function cancel() {
                if (available) {
                    speechSynthesis.cancel();
                    queue.forEach(q => q.cancel = true);
                }
                return getStatus();
            }
            SpeechSynthesis.cancel = cancel;
            function pause() { if (available)
                speechSynthesis.pause(); return getStatus(); }
            SpeechSynthesis.pause = pause;
            function resume() { if (available)
                speechSynthesis.resume(); return getStatus(); }
            SpeechSynthesis.resume = resume;
            (function (body, clickEventName) {
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
