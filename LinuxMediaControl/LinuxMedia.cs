using System.ComponentModel.DataAnnotations;
using Tmds.DBus;
using WebRemote.Models;


namespace LinuxMediaControl;
public class LinuxMedia : IWebMedia, IDisposable
{

    public LinuxMedia()
    {
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
                   string identity = await mediaPlayerRoot.GetAsync<string>("Identity");
                    {
                        
                        MediaPlayers.Add(new (identity, _connection.CreateProxy<IMediaPlayer>(service, "/org/mpris/MediaPlayer2")));
                    }
                    count++;
                }
            }
            SelectedMediaPlayer = count - 1;
        }
        return count;
    }
  
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
        AudioControl Audio = new();
        AudioRemoteMessage message = new(Audio.Volume, MediaPlayers.Select(players=> players.Item1).ToArray());
        Audio.Dispose();
        return message;
    }


    public string? GetPlaybackStatus(IMediaPlayer player)
    {
        return GetPlaybackStatusAsync(player).Result;
    }

    public async Task<string?> GetPlaybackStatusAsync(IMediaPlayer player) => await player.GetAsync<string?>("PlaybackStatus");
    

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
        AudioControl Audio = new();
        Audio.Volume = value;
        Audio.Dispose();
    }

    public void Dispose()
    {
        _connection.Dispose();
    }
}
