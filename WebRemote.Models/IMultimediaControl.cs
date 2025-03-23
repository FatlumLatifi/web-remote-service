using System.ComponentModel.DataAnnotations;


namespace WebRemote.Models;

public interface IMultimediaControl
{
     public bool ChangeSelectedMediaPlayer(string name);

     void SetVolume([Range(0,100)]int value);

     public IWebMedia? SelectedMediaPlayer { get; }

     public AudioRemoteMessage GetAudioMessage();

     public Task RefreshAsync();

}



public interface IWebMedia
{    
    void PlayPause();
    void Previous();
    void Next();
    void Stop();
}
