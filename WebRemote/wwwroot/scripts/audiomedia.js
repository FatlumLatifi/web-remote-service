import { WebRemoteMessage  } from "./main.js";
import { WebRemote } from './webremote.js';

export class AudioMedia extends HTMLElement
{
    constructor(){ super(); }
   
    connectedCallback() 
    {
        for (const button of this.getElementsByTagName("button")) 
        {
           button.onclick = () => 
           {
            console.log("sending " + button.getAttribute("key"));
               let message = new WebRemoteMessage(button.getAttribute("key"), false, true);
                WebRemote .webSocket.send(message.jsonString);
            }
        }
    }
}