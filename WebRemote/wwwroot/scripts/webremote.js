import { WebRemoteMessage, sendMouseClick } from './main.js';


export class WebRemote extends HTMLElement {

    socketStatus = this.querySelector("#socketStatus");
    reconnectButton = this.querySelector("button[reconnect]");
    constructor() {
        super();
        WebRemote.webSocket.onopen = () => this.setStatusColor();
        WebRemote.webSocket.onclose = () => this.setStatusColor();
        WebRemote.webSocket.onerror = () => this.setStatusColor();
        WebRemote.webSocket.onmessage = (message) => {
            alert(message.data);
        };
        this.reconnectButton.onclick = () => {
            WebRemote.webSocket = new WebSocket(`ws://${window.location.host}/ws`);
            this.reconnectButton.querySelector("svg").toggleAttribute("spin", true);
            setTimeout(
                () => {
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
                setTimeout(() => this.setStatusColor(), 1000);
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
        toggleTexting.onclick = () => {
            if (toggleTexting.hasAttribute("open")) { input.blur(); }
            else { input.focus(); }
            toggleTexting.toggleAttribute("open");
        };

        const openSettings = this.querySelector("#openSettings");
        const dialog = document.querySelector("dialog");
        openSettings.onclick = () => {
            if (dialog.open) { dialog.close(); }
            else {
                dialog.show();
                dialog.querySelector("settings-menu").opened();
            }
            openSettings.toggleAttribute("open");
        };

        input.onkeydown = (e) => {
            let isSpecial = (e.key.length > 2);
            const message = new WebRemoteMessage(e.key, false, true);
            if (isSpecial) {
                switch (e.key) {
                    case "Backspace":
                        message.data = "14";
                        break;
                    case "Enter":
                        message.data = "28";
                        break;
                    case "Unidentified":
                    default:
                        return;
                }
                WebRemote.webSocket.send(message.jsonString);
            }
        };

        input.oninput = () => {
            const message = new WebRemoteMessage(input.value, false, false);
            input.value = "";
            WebRemote.webSocket.send(message.jsonString);
        };

        const mouse1 = this.querySelector("#mouse1");
        mouse1.onclick = () => sendMouseClick(); // Left mouse
        const mouse3 = this.querySelector("#mouse3");
        mouse3.onclick = () => sendMouseClick("3"); // Right mouse
    }
}
