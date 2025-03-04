
namespace WebRemote.Models;
public record WebRemoteMessage(string data, bool isMouse, bool isSpecial);
public record AudioRemoteMessage(int volume, string[] players, int maxVolume = 100);

public record MediaControlMessage(string name, string action);