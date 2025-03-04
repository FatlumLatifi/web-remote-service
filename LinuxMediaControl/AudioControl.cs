using System;
using System.Runtime.InteropServices;

public class AudioControl : IDisposable
{
    private const string ALSA_LIB = "libasound.so.2";

    private IntPtr _handle;
    private IntPtr _elem;

    public long MaxVolume;
    // DllImport Declarations
    [DllImport(ALSA_LIB)]
    private static extern int snd_mixer_open(out IntPtr handle, int mode);

    [DllImport(ALSA_LIB)]
    private static extern int snd_mixer_attach(IntPtr handle, string name);

    [DllImport(ALSA_LIB)]
    private static extern int snd_mixer_selem_register(IntPtr mixer, IntPtr options, IntPtr classp);

    [DllImport(ALSA_LIB)]
    private static extern int snd_mixer_load(IntPtr mixer);

    [DllImport(ALSA_LIB)]
    private static extern void snd_mixer_close(IntPtr handle);

    [DllImport(ALSA_LIB)]
    private static extern void snd_mixer_selem_id_malloc(out IntPtr selem_id);

    [DllImport(ALSA_LIB)]
    private static extern void snd_mixer_selem_id_set_index(IntPtr selem_id, int idx);

    [DllImport(ALSA_LIB)]
    private static extern void snd_mixer_selem_id_set_name(IntPtr selem_id, string name);

    [DllImport(ALSA_LIB)]
    private static extern IntPtr snd_mixer_find_selem(IntPtr handle, IntPtr selem_id);

    [DllImport(ALSA_LIB)]
    private static extern int snd_mixer_selem_get_playback_volume_range(IntPtr elem, out long min, out long max);

    [DllImport(ALSA_LIB)]
    private static extern int snd_mixer_selem_get_playback_volume(IntPtr elem, int channel, out long value);

    [DllImport(ALSA_LIB)]
    private static extern int snd_mixer_selem_set_playback_volume_all(IntPtr elem, long volume);

    public AudioControl()
    {
        // Open mixer
        if (snd_mixer_open(out _handle, 0) < 0)
        {
            Console.WriteLine("Mixer open error");
            return;
        }

        // Attach mixer
        if (snd_mixer_attach(_handle, "default") < 0)
        {
            Console.WriteLine("Mixer attach error");
            snd_mixer_close(_handle);
            return;
        }

        // Register mixer
        if (snd_mixer_selem_register(_handle, IntPtr.Zero, IntPtr.Zero) < 0)
        {
            Console.WriteLine("Mixer register error");
            snd_mixer_close(_handle);
            return;
        }

        // Load mixer elements
        if (snd_mixer_load(_handle) < 0)
        {
            Console.WriteLine("Mixer load error");
            snd_mixer_close(_handle);
            return;
        }

        // Allocate simple element ID
        snd_mixer_selem_id_malloc(out IntPtr sid);
        snd_mixer_selem_id_set_index(sid, 0);
        snd_mixer_selem_id_set_name(sid, "Master");

        // Find the simple element
        _elem = snd_mixer_find_selem(_handle, sid);
        if (_elem == IntPtr.Zero)
        {
            Console.WriteLine("Mixer find error");
            snd_mixer_close(_handle);
            return;
        }

        // Get volume range
        snd_mixer_selem_get_playback_volume_range(_elem, out long minVolume, out MaxVolume);
    }

    public void SetVolume(long volume)
    {
        if (_elem == IntPtr.Zero)
        {
            Console.WriteLine("Mixer find error");
            snd_mixer_close(_handle);
            return;
        }
        // Set volume
        long setVolume = volume * MaxVolume / 100;
        snd_mixer_selem_set_playback_volume_all(_elem, setVolume);
    }

    public int GetVolume()
    {
        if (_elem == IntPtr.Zero)
        {
            Console.WriteLine("Mixer find error");
            snd_mixer_close(_handle);
            return -1;
        }
        // Get volume
        snd_mixer_selem_get_playback_volume(_elem, 0, out long volume);
        
        return (int)(100*volume/MaxVolume);
    }

    public void Dispose()
    {
        // Clean up
        snd_mixer_close(_handle);
    }
}
