import { WebRemoteMessage } from "./main.js";
import { WebRemote } from './webremote.js';

export class TouchPad extends HTMLElement 
{
    constructor() { super(); };
    #startNoMove = false;
    #message = new WebRemoteMessage("", true, false);
    static sensitivity = Number.parseFloat(localStorage.getItem("mouse-sensitivity") ?? "1");
    #lastStart = { x: 0, y: 0 };

    connectedCallback() 
    {
        this.ontouchstart = (e) => {
            this.#lastStart.x = e.touches[0].clientX;
            this.#lastStart.y = e.touches[0].clientY;
            this.#startNoMove = true;
        };
        this.ontouchmove = (e) => 
        {
            if(this.#startNoMove == true) {this.#startNoMove = false; } 
            this.#message.isSpecial = false;
            if (e.touches.length == 2)
            {
                if (e.touches[0].clientY > this.#lastStart.y) { this.#message.data = "4"; }
                else { this.#message.data = "5"; }
                this.#message.isSpecial = true;
            }
            else 
            {
                const distanceX = Math.round((e.touches[0].clientX - this.#lastStart.x) * TouchPad.sensitivity);
                const distanceY = Math.round((e.touches[0].clientY - this.#lastStart.y) * TouchPad.sensitivity);
                this.#message.data = `${distanceX},${distanceY}`;
            }
            WebRemote.webSocket.send(this.#message.jsonString);
            this.#lastStart.x = e.touches[0].clientX;
            this.#lastStart.y = e.touches[0].clientY;
        };
        this.ontouchend = (e) =>
        {
                if(this.#startNoMove == true)
              { 
                this.#message.data = "1"; // Left Click
                this.#message.isSpecial = true;
                WebRemote.webSocket.send(this.#message.jsonString);
              }
        };
    };
}
