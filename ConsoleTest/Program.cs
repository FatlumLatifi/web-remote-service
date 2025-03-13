using LinuxMediaControl;
using Tmds.DBus;

var _connection = Connection.Session;
  string[]? services;

        try { services = await _connection.ListServicesAsync(); } 
        catch { Console.WriteLine("services null"); return; }
        
        if (services is not null)
        {
            foreach(string service in services)
            {
                
                 
                if (service.StartsWith("org.mpris.MediaPlayer2."))
                {
                Console.WriteLine($"==========|Service: {service}|==============");
                    var mediaPlayerRoot = _connection.CreateProxy<IMediaPlayerRoot>(service, "/org/mpris/MediaPlayer2");
                    var mediaPlayer = _connection.CreateProxy<IMediaPlayer>(service, "/org/mpris/MediaPlayer2");
                    double volume = await mediaPlayer.GetAsync<double>("Volume");
                    string playbackStatus = await mediaPlayer.GetAsync<string>("PlaybackStatus");
                Console.WriteLine($"Volume is: {volume}, PlaybackStatus: {playbackStatus}");
                    Console.WriteLine("setting volume to 0.99");
                await mediaPlayer.SetAsync("Volume", 0.99);
                PrintProps(await mediaPlayer.GetAllAsync());
                }

            }
        }

using AudioControl ac = new();
ac.SetVolume(99);

static void PrintProps(IDictionary<string, object> properties)
{
 foreach(var prop in properties)
            {
            Console.WriteLine($"{prop.Key}: {prop.Value}");
            }
}


        