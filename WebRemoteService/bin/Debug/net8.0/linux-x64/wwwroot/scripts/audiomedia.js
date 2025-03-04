export class AudioMedia extends HTMLElement
{

    #audioMessage = { "volume": 0, "players": [""], "maxVolume": 100 };

    volumeControl = this.querySelector("input[type=range]");

    selectedPlayer = this.querySelector("select");


    set audioMessage(audioMessage)
    {
        this.#audioMessage = audioMessage;
        this.update();
    };

    set audioMessageFromJsonString(jsonString)
    {
        this.#audioMessage = JSON.parse(jsonString);
        this.update();
    };
    constructor() 
    {
        super();

        this.selectedPlayer.onchange = () => {
        }
        this.volumeControl.onchange = async () =>
        {
            const req = await fetch(`/audio/volume/${this.volumeControl.value}`, { method: "POST" });
            if (req.ok != true)
            {
                alert(await req.text());
            }
        }

        this.querySelectorAll("button").forEach(button=>
        {
            button.onclick = async () => 
            {
               // const body = { "name": button.dataset.action}
                const req = await fetch("/audio", 
                    { 
                        method: "POST", 
                        headers: {
                            "Content-Type": "application/json",
                          },
                        body: JSON.stringify({"name": this.selectedPlayer.value.trim(), "action":button.dataset.action })
                    });
                if (req.ok == false)
                {
                    alert(await req.text());
                }
            }
        }
        )
    }

    update()
    {
        this.volumeControl.setAttribute("max", this.#audioMessage.maxVolume);
        this.volumeControl.setAttribute("value", this.#audioMessage.volume);
        this.selectedPlayer.innerHTML = "";
        for(let player of this.#audioMessage.players)
        {
            this.selectedPlayer.innerHTML += `<option>${player}</option>`;
        }
    }
}