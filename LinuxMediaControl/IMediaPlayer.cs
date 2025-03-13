using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Tmds.DBus;
using WebRemote.Models;

namespace LinuxMediaControl;

public class LinuxMediaPlayer : IWebMedia
{
    public IMediaPlayer _mediaPlayer;
    public LinuxMediaPlayer(IMediaPlayer dbusMediaPlayer)
    {
        _mediaPlayer = dbusMediaPlayer;
    }
    public async void Next()=> await _mediaPlayer.NextAsync();
    public async void PlayPause() => await _mediaPlayer.PlayPauseAsync();
    public async void Previous() => await  _mediaPlayer.PreviousAsync();
    public async void Stop() => await _mediaPlayer.StopAsync();
}

[DBusInterface("org.mpris.MediaPlayer2.Player")]
public interface IMediaPlayer : IDBusObject
{
    Task PlayPauseAsync();
    Task NextAsync();
    Task PreviousAsync();
    Task StopAsync();

    Task<T> GetAsync<T>(string prop);
    Task SetAsync(string prop, object val);

}

// Interface for the org.mpris.MediaPlayer2 interface (root object)
[DBusInterface("org.mpris.MediaPlayer2")]
public interface IMediaPlayerRoot : IDBusObject
{
    Task<T> GetAsync<T>(string prop);
    Task SetAsync(string prop, object val);
    Task<IDictionary<string, object>> GetAllAsync();
}