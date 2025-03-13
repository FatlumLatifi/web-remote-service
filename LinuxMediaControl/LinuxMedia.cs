using System.ComponentModel.DataAnnotations;
using Tmds.DBus;
using WebRemote.Models;


namespace LinuxMediaControl;

public class LinuxMedia :  IMultimediaControl, IDisposable
{

    public LinuxMedia()
    {
        _connection = Connection.Session;
        MediaPlayers = new();
        
        Task.Run(async () => await CheckMediaPlayersAsync()).GetAwaiter().GetResult();
    }

    public async Task RefreshAsync()
    {
        await CheckMediaPlayersAsync();
    }


    /// <summary>
    /// 
    /// </summary>
    /// <returns>Returns true if it found anything</returns>
    public async Task<int> CheckMediaPlayersAsync()
    {
        string[]? services;
        try { services = await _connection.ListServicesAsync(); } 
        catch { return 0; }
        int count = 0;
        MediaPlayers.Clear();
        if (services is not null)
        {
            foreach(string service in services)
            {
                if (service.StartsWith("org.mpris.MediaPlayer2."))
                {
                   IMediaPlayerRoot mediaPlayerRoot = _connection.CreateProxy<IMediaPlayerRoot>(service, "/org/mpris/MediaPlayer2");
                   string identity = await mediaPlayerRoot.GetAsync<string>("Identity");
                   LinuxMediaPlayer mediaPlayer = new(_connection.CreateProxy<IMediaPlayer>(service, "/org/mpris/MediaPlayer2"));
                   double volume = 1.00;
                   try { volume = await mediaPlayer._mediaPlayer.GetAsync<double>("Volume"); } catch { }
                   
                   MediaPlayers.Add(new(identity, mediaPlayer) 
                   { 
                    playbackStatus = await mediaPlayer._mediaPlayer.GetAsync<string?>("PlaybackStatus") ?? "Playing..",
                    volume = volume
                    });
                    
                   count++;
                }
            }
            SelectedMediaPlayer = count - 1;
        }
        return count;
    }


  
    private Connection _connection;
    public List<WebRemotePlayer> MediaPlayers;

    private int SelectedMediaPlayer;
    IWebMedia? IMultimediaControl.SelectedMediaPlayer => MediaPlayers[SelectedMediaPlayer].WebMedia;

    public bool ChangeSelectedMediaPlayer(string name)
    {
        for(int i = 0; i < MediaPlayers.Count; i++)
        {
            if (name.Equals(MediaPlayers[i].name, StringComparison.InvariantCultureIgnoreCase))
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
        AudioRemoteMessage message = new(Audio.Volume, MediaPlayers);
        Audio.Dispose();
        return message;
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
