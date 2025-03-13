using System.ComponentModel.DataAnnotations;

namespace WebRemote.Models;

public interface IWebRemoteControl : IDisposable
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

