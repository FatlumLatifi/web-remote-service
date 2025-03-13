using Tmds.DBus;

namespace LinuxMediaControl;


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