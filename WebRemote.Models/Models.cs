
using System.Text.Json.Serialization;

namespace WebRemote.Models;
public record WebRemoteMessage(string data, bool isMouse, bool isSpecial);
public record AudioRemoteMessage(int volume, List<WebRemotePlayer> players, int maxVolume = 100);

public record MediaControlMessage(string name, string action);


public class WebRemotePlayer
{
    public WebRemotePlayer(string name, IWebMedia? webMedia = null)
    {
        this.name = name;
        if (webMedia is not null)
        {
            this.WebMedia = webMedia; 
        }
    }
    
    public string name { get; set; }
    public string playbackStatus { get; set; } = "Playing";
    public double volume { get;set; } = 1.00;

    [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
    public IWebMedia? WebMedia { get; set; }

}