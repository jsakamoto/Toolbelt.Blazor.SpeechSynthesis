var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : new P(function (resolve) { resolve(result.value); }).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
var Toolbelt;
(function (Toolbelt) {
    var Blazor;
    (function (Blazor) {
        var SpeechSynthesis;
        (function (SpeechSynthesis) {
            const s = window.speechSynthesis;
            const onVoicesChanged = s.getVoices().length == 0 ? new Promise(resolve => s.addEventListener('voiceschanged', () => resolve())) : null;
            function refresh(objWrapper) {
                objWrapper.invokeMethodAsync('UpdateStatus', s.paused, s.pending, s.speaking);
            }
            SpeechSynthesis.refresh = refresh;
            function getVoices() {
                return __awaiter(this, void 0, void 0, function* () {
                    if (onVoicesChanged != null) {
                        yield onVoicesChanged;
                    }
                    return s.getVoices().map(v => ({
                        _Default: v.default,
                        _Lang: v.lang,
                        _LocalService: v.localService,
                        _Name: v.name,
                        _VoiceURI: v.voiceURI
                    }));
                });
            }
            SpeechSynthesis.getVoices = getVoices;
            function speak(arg) {
                const utterance = new SpeechSynthesisUtterance();
                if (typeof arg == 'string')
                    utterance.text = arg;
                else {
                    if (arg.voice != null) {
                        arg.voice = s.getVoices().find(v => v.voiceURI == arg.voice.voiceURI);
                    }
                    Object.assign(utterance, arg);
                }
                s.speak(utterance);
            }
            SpeechSynthesis.speak = speak;
            function cancel() { s.cancel(); }
            SpeechSynthesis.cancel = cancel;
            function pause() { s.pause(); }
            SpeechSynthesis.pause = pause;
            function resume() { s.resume(); }
            SpeechSynthesis.resume = resume;
        })(SpeechSynthesis = Blazor.SpeechSynthesis || (Blazor.SpeechSynthesis = {}));
    })(Blazor = Toolbelt.Blazor || (Toolbelt.Blazor = {}));
})(Toolbelt || (Toolbelt = {}));
//# sourceMappingURL=script.js.map