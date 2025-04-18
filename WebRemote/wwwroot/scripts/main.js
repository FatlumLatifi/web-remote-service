import { TouchPad } from './touchpad.js';
import { SettingsMenu } from './settings.js';
import { AudioMedia } from './audiomedia.js';
import { WebRemote } from './webremote.js';


export function sendMouseClick(numberChar = "1") {
    const message = new WebRemoteMessage(numberChar, true, true);
    WebRemote.webSocket.send(message.jsonString);
}
// A message with isSpecial set to true does mouse clicks
// in data field: "1" is left, "2" is middle, "3" is right, "4" is wheel up, "5" is wheel down.
// Else you send distance x and y traveled, seperated by a single comma: x,y


export function registerAllCustomElements() {
    customElements.define('web-remote', WebRemote);
    customElements.define('touch-pad', TouchPad);
    customElements.define('settings-menu', SettingsMenu);
    customElements.define('audio-media', AudioMedia);
}

export class WebRemoteMessage {
    
    constructor(data, isMouse = false, isSpecial = false) {
        this.data = data;
        this.isMouse = isMouse;
        this.isSpecial = isSpecial;
    };
    get jsonString() { return JSON.stringify(this); }
}