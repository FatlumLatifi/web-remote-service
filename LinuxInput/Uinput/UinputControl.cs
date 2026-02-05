using System.Runtime.InteropServices;
using WebRemote.Models;


namespace LinuxInput;

public class UinputControl : IWebRemoteControl
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    struct uinput_setup
    {
        public ushort bustype, vendor, product, version;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
        public string name;
        public uint ff_effects_max;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct input_event
    {
        public IntPtr tv_sec, tv_usec;
        public ushort type, code;
        public int value;
    }

    [DllImport("libc", SetLastError = true)] static extern int open(string pathname, int flags);
    [DllImport("libc", SetLastError = true)] static extern int ioctl(int fd, ulong request, int arg);
    [DllImport("libc", SetLastError = true)] static extern int ioctl(int fd, ulong request, ref uinput_setup setup);
    [DllImport("libc", SetLastError = true)] static extern int write(int fd, ref input_event ev, int count);
    [DllImport("libc")] static extern int close(int fd);

    // Constants
    const int O_WRONLY = 1, O_NONBLOCK = 2048;
    const ushort EV_SYN = 0, EV_KEY = 1, EV_REL = 2;
    const ushort Xplane = 0, Yplane = 1;

    // Mouse Button Codes
    public const ushort BTN_LEFT = 0x110;
    public const ushort BTN_RIGHT = 0x111;
    public const ushort BTN_MIDDLE = 0x112;

    const ulong UI_SET_EVBIT = 0x40045564;
    const ulong UI_SET_KEYBIT = 0x40045565;
    const ulong UI_SET_RELBIT = 0x40045566;
    const ulong UI_DEV_SETUP = 0x405c5503;
    const ulong UI_DEV_CREATE = 0x5501;

    public static int? _fd = null;

    public UinputControl()
    {
        if (_fd != null) { CloseDevice(); return; }

        _fd = open("/dev/uinput", O_WRONLY | O_NONBLOCK);
        if (_fd < 0)
        {
            Console.WriteLine("Error: Open /dev/uinput failed.");
            return;
        }

        ioctl(_fd.Value, UI_SET_EVBIT, EV_REL);
        ioctl(_fd.Value, UI_SET_RELBIT, Xplane);
        ioctl(_fd.Value, UI_SET_RELBIT, Yplane);

        // Registering Buttons
        ioctl(_fd.Value, UI_SET_EVBIT, EV_KEY);
        ioctl(_fd.Value, UI_SET_KEYBIT, BTN_LEFT);
        ioctl(_fd.Value, UI_SET_KEYBIT, BTN_RIGHT);
        ioctl(_fd.Value, UI_SET_KEYBIT, BTN_MIDDLE);
        for (ushort i = 1; i < 255; i++)
        {
            ioctl(_fd.Value, UI_SET_KEYBIT, i);
        }

        var setup = new uinput_setup
        {
            name = "CSharp-Virtual-Mouse",
            bustype = 0x03,
            vendor = 0x1,
            product = 0x1
        };

        ioctl(_fd.Value, UI_DEV_SETUP, ref setup);
        ioctl(_fd.Value, UI_DEV_CREATE, 0);

        Console.WriteLine("Device created.");
        Thread.Sleep(1000);
    }


    static void SendEvent(ushort type, ushort code, int value)
    {
        if (_fd.HasValue is false) return;
        var ev = new input_event { type = type, code = code, value = value };
        write(_fd.Value, ref ev, Marshal.SizeOf(ev));
    }

    public static void MoveMouse(int xplanemove, int yplanemove)
    {
       SendEvent(EV_REL, Xplane, xplanemove); 
       SendEvent(EV_SYN, 0, 0);
       SendEvent(EV_REL, Yplane, yplanemove);
       SendEvent(EV_SYN, 0, 0);
       // SendEvent(EV_SYN, 0, 0);
    }

    /// <summary>
    /// Simulates a mouse click.
    /// </summary>
    /// <param name="buttonCode">Use UInputser.BTN_LEFT, BTN_RIGHT, etc.</param>
    public static void ClickMouse(ushort buttonCode)
    {
        PressRawKey(buttonCode);
    }


    public static void PressRawKey(ushort keycode)
    {
        SendEvent(EV_KEY, keycode, 1);
        SendEvent(EV_SYN, 0, 0);
        Thread.Sleep(7); // Very short delay for the OS to register
        SendEvent(EV_KEY, keycode, 0);
        SendEvent(EV_SYN, 0, 0);
        Thread.Sleep(7);
    }

    public void TypeText(string text)
    {
        foreach (char c in text)
        {
            TypeUnicode(c);
        }

    }

    private static ushort GetHexKeycode(char c)
    {
        return c switch
        {
            '0' => 11,
            '1' => 2,
            '2' => 3,
            '3' => 4,
            '4' => 5,
            '5' => 6,
            '6' => 7,
            '7' => 8,
            '8' => 9,
            '9' => 10,
            'a' => 30,
            'b' => 48,
            'c' => 46,
            'd' => 32,
            'e' => 18,
            'f' => 33,
            _ => 0
        };
    }
    public static void TypeUnicode(char c)
    {
        // Convert character to hex string (e.g. 'ë' becomes "eb")
        string hex = ((int)c).ToString("x");

        // 1. Start the sequence: Ctrl + Shift + U
        SendEvent(EV_KEY, 29, 1); // L_CTRL Down
        SendEvent(EV_KEY, 42, 1); // L_SHIFT Down
        PressRawKey(22);          // 'U' Key
        SendEvent(EV_KEY, 42, 0); // L_SHIFT Up
        SendEvent(EV_KEY, 29, 0); // L_CTRL Up
        SendEvent(EV_SYN, 0, 0);

        // 2. Type the hex digits
        foreach (char digit in hex)
        {
            ushort code = GetHexKeycode(digit);
            if (code != 0) PressRawKey(code);
        }

        // 3. Complete the sequence: Press Enter
        PressRawKey(28);
    }

    public static void CloseDevice()
    {
        if (_fd.HasValue is false) return;
        ioctl(_fd.Value, 0x5502, 0); // UI_DEV_DESTROY
        close(_fd.Value);
        _fd = null;
    }

    public void SendKey(int keySequence)
    {
        PressRawKey((ushort)keySequence);
    }



    public void MoveBy(int x, int y)
    {
        MoveMouse(x, y);
    }

    public void SendClick(int button)
    {
        ushort buttonCode;
        switch (button)
        {
            case 1: buttonCode = BTN_LEFT; break;
            case 2: buttonCode = BTN_MIDDLE; break;
            case 3: buttonCode = BTN_RIGHT; break;
            default: return; // Invalid button
        }
        ClickMouse(buttonCode);
    }

    public void Dispose()
    {
        CloseDevice();
    }
}