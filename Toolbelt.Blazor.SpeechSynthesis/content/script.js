var Toolbelt;
(function (Toolbelt) {
    var Blazor;
    (function (Blazor) {
        var SpeechSynthesis;
        (function (SpeechSynthesis) {
            const s = window.speechSynthesis || { paused: false, pending: false, speaking: false };
            const available = typeof s.getVoices !== 'undefined';
            const onVoicesChanged = (available && s.getVoices().length === 0) ? new Promise(resolve => s.addEventListener('voiceschanged', () => resolve())) : Promise.resolve();
            let queue = [];
            function refresh(objWrapper) {
                objWrapper.invokeMethodAsync('UpdateStatus', available, s.paused, s.pending, s.speaking);
            }
            SpeechSynthesis.refresh = refresh;
            function getVoices() {
                if (!available)
                    return Promise.resolve([]);
                return onVoicesChanged
                    .then(() => s.getVoices().map(v => ({
                    default: v.default,
                    lang: v.lang,
                    localService: v.localService,
                    name: v.name,
                    voiceURI: v.voiceURI
                })));
            }
            SpeechSynthesis.getVoices = getVoices;
            function speak(sRef, arg, uRef) {
                if (!available)
                    return;
                const u = new SpeechSynthesisUtterance();
                if (arg.voice !== null)
                    arg.voice = s.getVoices().find(v => v.voiceURI === arg.voice.voiceURI);
                Object.assign(u, arg);
                const types = ["boundary", "end", "error", "mark", "pause", "resume", "start"];
                const f = function (ev) {
                    refresh(sRef);
                    uRef.invokeMethodAsync('InvokeEvent', ev.type);
                    if (ev.type === 'end' || ev.type === 'error' || ev.type === 'cancel') {
                        types.forEach(t => u.removeEventListener(t, f));
                        queue = queue.filter(q => q.f !== f);
                    }
                };
                types.forEach(t => u.addEventListener(t, f));
                queue.push({ f });
                s.speak(u);
                setTimeout(() => {
                    const cancellers = queue.filter(q => q.cancel === true);
                    cancellers.forEach(c => c.f({ type: 'cancel' }));
                }, 0);
            }
            SpeechSynthesis.speak = speak;
            function cancel() {
                if (!available)
                    return;
                s.cancel();
                queue.forEach(q => q.cancel = true);
            }
            SpeechSynthesis.cancel = cancel;
            function pause() { if (available)
                s.pause(); }
            SpeechSynthesis.pause = pause;
            function resume() { if (available)
                s.resume(); }
            SpeechSynthesis.resume = resume;
        })(SpeechSynthesis = Blazor.SpeechSynthesis || (Blazor.SpeechSynthesis = {}));
    })(Blazor = Toolbelt.Blazor || (Toolbelt.Blazor = {}));
})(Toolbelt || (Toolbelt = {}));
//# sourceMappingURL=script.js.map