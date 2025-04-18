
using System.Text.Json.Serialization;

namespace WebRemote.Models;
public record WebRemoteMessage(string data, bool isMouse, bool isSpecial);
public record MediaControlMessage(string name, string action);
