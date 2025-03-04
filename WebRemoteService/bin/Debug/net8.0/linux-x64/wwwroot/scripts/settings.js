import { TouchPad } from "./touchpad.js";

export class SettingsMenu extends HTMLElement
{
    constructor() { super(); }

    connectedCallback()
    {
        this.sensitivityRange = this.querySelector("input[type=range]");
        this.sensitivityRange.onchange = () =>
        {
            TouchPad.sensitivity = Number.parseFloat(this.sensitivityRange.value);
            this.opened();
            localStorage.setItem("mouse-sensitivity", TouchPad.sensitivity);
        }
    }

    opened()
    {
        this.sensitivityRange.value = TouchPad.sensitivity;
        this.querySelector("ins").textContent = TouchPad.sensitivity;
    }
}