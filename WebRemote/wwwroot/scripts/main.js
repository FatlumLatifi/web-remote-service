import { TouchPad } from './touchpad.js';
import { SettingsMenu } from './settings.js';
import { AudioMedia } from './audiomedia.js';

export class WebRemote extends HTMLElement {

    socketStatus = this.querySelector("#socketStatus");
    reconnectButton =  this.querySelector("button[reconnect]");
    constructor() 
    {
        super();
        WebRemote.webSocket.onopen = () => this.setStatusColor();
        WebRemote.webSocket.onclose = () => this.setStatusColor();
        WebRemote.webSocket.onerror = () => this.setStatusColor(); 
        WebRemote.webSocket.onmessage = (message) =>
        {
            document.querySelector("audio-media").audioMessageFromJsonString = message.data;
        };
        this.reconnectButton.onclick = () =>
        {
            WebRemote.webSocket = new WebSocket(`ws://${window.location.host}/ws`);
            this.reconnectButton.querySelector("svg").toggleAttribute("spin", true);
            setTimeout(
                ()=> {
                    this.setStatusColor(); 
                    this.reconnectButton.querySelector("svg").toggleAttribute("spin", false);
                }, 
            1000);
        };
    }

    setStatusColor() {
        switch (WebRemote.webSocket.readyState) {

            case WebSocket.CONNECTING:
            case WebSocket.CLOSING:
                this.socketStatus.setAttribute("fill", "orange");
                this.reconnectButton.style.display = "none";
                setTimeout(()=>this.setStatusColor(), 1000);
                return;
            case WebSocket.OPEN:
                this.socketStatus.setAttribute("fill", "green");
                this.reconnectButton.style.display = "none"; 
                return;
            case WebSocket.CLOSED:
                this.socketStatus.setAttribute("fill", "red");
                this.reconnectButton.style.display = "block";
                return;
            default:
                this.socketStatus.setAttribute("fill", "transparent");
                this.reconnectButton.style.display = "block";
                return;
        }
    }

    static webSocket = new WebSocket(`ws://${window.location.host}/ws`);

    connectedCallback() {
        this.setStatusColor();

        const input = this.querySelector("#textIn");
        const toggleTexting = this.querySelector("#toggleTexting");
        
        input.addEventListener("focusout", (e) => {
            toggleTexting.toggleAttribute("open", false);
          });
        toggleTexting.onclick = ()  => { 
            if (toggleTexting.hasAttribute("open")) { input.blur();}
            else { input.focus(); }
            toggleTexting.toggleAttribute("open");
        };

        const openSettings = this.querySelector("#openSettings");
        const dialog = document.querySelector("dialog");
        openSettings.onclick = () => {
            if (dialog.open) { dialog.close();  }
            else {
                dialog.show();
                dialog.querySelector("settings-menu").opened();
            }
            openSettings.toggleAttribute("open");
        }

        input.onkeydown = (e) => 
        {
            let isSpecial = (e.key.length > 2);
            const message = new WebRemoteMessage(e.key, false, true);
            if (isSpecial) 
            {
                switch (e.key) {
                    case "Backspace":
                        message.data = "BackSpace";
                        break;
                    case "Enter":
                        message.data = "Return";
                        break;
                    case "Unidentified":
                    default:
                        return;
                }
                WebRemote.webSocket.send(message.jsonString);
            }             
        }; 

        input.oninput = () =>
        {
            const message = new WebRemoteMessage(input.value, false, false);
            input.value = "";
            WebRemote.webSocket.send(message.jsonString);
        };

        const mouse1 = this.querySelector("#mouse1");
        mouse1.onclick = () => sendMouseClick(); // Left mouse
        const mouse2 = this.querySelector("#mouse3");
        mouse2.onclick = () => sendMouseClick("3"); // Right mouse
    }
}


export function sendMouseClick(numberChar = "1") {
    const message = new WebRemoteMessage(numberChar, true, true);
    WebRemote.webSocket.send(message.jsonString);
}

// A message with isSpecial set to true does mouse clicks
// in data field: "1" is left, "2" is middle, "3" is right, "4" is wheel up, "5" is wheel down.
// Else you send distance x and y traveled, seperated by a single comma: x,y
export class WebRemoteMessage {
    
    constructor(data, isMouse = false, isSpecial = false) {
        this.data = data;
        this.isMouse = isMouse;
        this.isSpecial = isSpecial;
    };
    get jsonString() { return JSON.stringify(this); }
}


export function registerAllCustomElements() {
    customElements.define('web-remote', WebRemote);
    customElements.define('touch-pad', TouchPad);
    customElements.define('settings-menu', SettingsMenu);
    customElements.define('audio-media', AudioMedia);
}