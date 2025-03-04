using System;
using System.Text.Json.Serialization;
using Tmds.DBus;
using WebRemote.Models;

namespace LinuxMediaControl;


[DBusInterface("org.mpris.MediaPlayer2.Player")]
public interface IMediaPlayer : IDBusObject
{
    Task PlayPauseAsync();
    Task NextAsync();
    Task PreviousAsync();
    Task StopAsync();
   Task<IDictionary<string, object>> GetAllAsync();
}




   // Interface for the org.mpris.MediaPlayer2 interface (root object)
    [DBusInterface("org.mpris.MediaPlayer2")]
    public interface IMediaPlayerRoot : IDBusObject
    {
        Task<IDictionary<string, object>> GetAllAsync();
    }

