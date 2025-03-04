using System.ComponentModel.DataAnnotations;

namespace WebRemote.Models;

public interface IWebRemoteServer : IDisposable
{
    public IWebRemoteKeyboard Keyboard { get; }
    public IWebRemoteMouse Mouse { get; }

}

public interface IWebRemoteKeyboard
{
    void SendKey(string keySequence);
    void TypeText(string text);
}

public interface IWebRemoteMouse
{
    void MoveBy(int x, int y);
    void SendClick(int button);
}

public interface IWebMedia
{
       public AudioRemoteMessage GetAudioMessage();
    void SetVolume([Range(0,100)]int value);
    void PlayPause();
    void Previous();
    void Next();
    void Stop();
}