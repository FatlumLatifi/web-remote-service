using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;
using LinuxMediaControl;
using Tmds.DBus;
using WebRemote.Models;


namespace LinuxMediaControl;
public class LinuxMedia : IWebMedia, IDisposable
{

    public LinuxMedia()
    {
        Audio = new();
        _connection = Connection.Session;
        MediaPlayers = new();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>Returns true if it found anything</returns>
    public async Task<int> CheckMediaPlayersAsync()
    {
        string[]? services;
        try { services = await _connection.ListServicesAsync();} 
        catch { return 0; }
        int count = 0;
        if (services is not null)
        {
            foreach(string service in services)
            {
                
                if (service.StartsWith("org.mpris.MediaPlayer2."))
                {
                   var mediaPlayerRoot = _connection.CreateProxy<IMediaPlayerRoot>(service, "/org/mpris/MediaPlayer2");
                    var properties = await mediaPlayerRoot.GetAllAsync();
                    if (properties["Identity"]?.ToString() is not null && (MediaPlayers.Exists(x=> x.Item1 == properties["Identity"].ToString()) == false))
                    {
                        MediaPlayers.Add(new (properties["Identity"].ToString()!, _connection.CreateProxy<IMediaPlayer>(service, "/org/mpris/MediaPlayer2")));
                    }
                    count++;
                }
            }
            SelectedMediaPlayer = count - 1;
        }
        return count;
    }
    public AudioControl Audio { get; private set; }

    private Connection _connection;
    public List<(string, IMediaPlayer)> MediaPlayers;

    private int SelectedMediaPlayer;
    

    public bool ChangeSelectedMediaPlayer(string name)
    {
        for(int i = 0; i < MediaPlayers.Count; i++)
        {
            if (name.Equals(MediaPlayers[i].Item1, StringComparison.InvariantCultureIgnoreCase))
            {
                SelectedMediaPlayer = i;
                return true;
            }
        }
        return false;
    }



    public AudioRemoteMessage GetAudioMessage()
    {
       return new AudioRemoteMessage((int)Audio.GetVolume(), MediaPlayers.Select(players=> players.Item1).ToArray());
    }


    public string? GetPlaybackStatus(IMediaPlayer player)
    {
        return GetPlaybackStatusAsync(player).Result;
    }

    public async Task<string?> GetPlaybackStatusAsync(IMediaPlayer player)
    {
        var properties = await player.GetAllAsync();
        Console.WriteLine("Properties Count" + properties.Count.ToString());
        return properties["PlaybackStatus"].ToString();
    }

    public async void Next()
    {
        if (MediaPlayers[SelectedMediaPlayer].Item2 is null)
        {
          SelectedMediaPlayer = 0;
          if (MediaPlayers[SelectedMediaPlayer].Item2 is null) { return; }
        }
        await MediaPlayers[SelectedMediaPlayer].Item2.NextAsync();
    }

    public async void PlayPause()
    {
          if (MediaPlayers[SelectedMediaPlayer].Item2 is null)
        {
          SelectedMediaPlayer = 0;
          if (MediaPlayers[SelectedMediaPlayer].Item2 is null) { return; }
        }
        await MediaPlayers[SelectedMediaPlayer].Item2.PlayPauseAsync();
    }

    public async void Previous()
    {
           if (MediaPlayers[SelectedMediaPlayer].Item2 is null)
        {
          SelectedMediaPlayer = 0;
          if (MediaPlayers[SelectedMediaPlayer].Item2 is null) { return; }
        }
        await MediaPlayers[SelectedMediaPlayer].Item2.PreviousAsync();
    }

    public async void Stop()
    {
           if (MediaPlayers[SelectedMediaPlayer].Item2 is null)
        {
          SelectedMediaPlayer = 0;
          if (MediaPlayers[SelectedMediaPlayer].Item2 is null) { return; }
        }
        await MediaPlayers[SelectedMediaPlayer].Item2.StopAsync();
    }
    public void SetVolume([Range(0, 100)] int value)
    {
        Audio.SetVolume(value);
    }

 


    public void Dispose()
    {
        _connection.Dispose();
        Audio.Dispose();
    }
}
