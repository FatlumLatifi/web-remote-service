
using System.Runtime.InteropServices;
using WebRemote.Models;

namespace LinuxInput.X11;

public class X11WebRemoteServer : IWebRemoteServer
{
    private IntPtr _xdo;

    public X11WebRemoteServer()
    {
        _xdo = xdo_new(IntPtr.Zero);
        Keyboard = new X11Keyboard(_xdo);
        Mouse = new X11Mouse(_xdo);
    }

    [DllImport("libxdo")]
    private static extern IntPtr xdo_new(IntPtr display);

    [DllImport("libxdo")]
    private static extern void xdo_free(IntPtr xdo);


    public IWebRemoteKeyboard Keyboard { get; private set; }

    public IWebRemoteMouse Mouse { get; private set; }

    public void Dispose()
    {
        xdo_free(_xdo);
    }
}
