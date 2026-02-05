using System.ComponentModel.DataAnnotations;

namespace WebRemote.Models;

public interface IWebRemoteControl : IDisposable
{
 
    void SendKey(int keySequence);
    void TypeText(string text);

    void MoveBy(int x, int y);
    void SendClick(int button);
}

