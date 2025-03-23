using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebRemote.Models;

namespace WindowsInputRemote
{
    public class WindowsMedia : IMultimediaControl
    {

        public WaveOutEvent _waveEvent { get; set; } = new ();

        public IWebMedia? SelectedMediaPlayer { get; set; } 

        public bool ChangeSelectedMediaPlayer(string name)
        {
            SelectedMediaPlayer = new NAudioWave(_waveEvent);
            return false;
        }

        public AudioRemoteMessage GetAudioMessage()
        {
            return new AudioRemoteMessage(50, []);
        }

        public Task RefreshAsync()
        {
            return Task.CompletedTask;
        }

        public void SetVolume([Range(0, 100)] int value)
        {
            using (var enumerator = new MMDeviceEnumerator())
            {
                var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                device.AudioEndpointVolume.MasterVolumeLevelScalar = value / 100f;
            }

        }
    }

    public class NAudioWave : IWebMedia
    {
        public NAudioWave(WaveOutEvent waveEvent) { _waveEvent = waveEvent; }

        public WaveOutEvent _waveEvent { get; set; }

        public void Next()
        {
        }

        public void PlayPause()
        {
            if (_waveEvent.PlaybackState == PlaybackState.Playing)
            {
                _waveEvent.Pause();
            }
            else
            {
                _waveEvent.Play();
            }
        }

        public void Previous()
        {
        }

        public void Stop()
        {
            _waveEvent.Stop();
        }
    }
}
