using System;
using System.Runtime.InteropServices;
using WebRemote.Models;

namespace LinuxInput.X11;
public class X11Keyboard : IWebRemoteKeyboard
{
    public IntPtr _xdo;

       public X11Keyboard(IntPtr xdo)
    {
       _xdo = xdo;

    }

    [DllImport("libxdo")]
    private static extern int xdo_send_keysequence_window(IntPtr xdo, IntPtr window, string sequence, uint delay);

    [DllImport("libxdo")]
    private static extern int xdo_enter_text_window(IntPtr xdo, IntPtr window, string text, uint delay);

    /// <summary>
    /// Send a click for a specific mouse button at the current mouse location.  
    /// </summary>
    /// <param name="xdo">xdo pointer</param>
    /// <param name="window">The window you want to send the event to or CURRENTWINDOW</param>
    /// <param name="button">button The mouse button. Generally, 1 is left, 2 is middle, 3 is right, 4 is wheel up, 5 is wheel down.</param>
    /// <returns></returns>
    [DllImport("libxdo")]
    public static extern int xdo_click_window(IntPtr xdo, IntPtr window, int button);

    public void TypeText(string text)
    {
        if (xdo_enter_text_window(_xdo, IntPtr.Zero, text, 12000) != 0)
            throw new Exception("Typing failed");
    }

    public void SendKey(string keySequence)
    {
        if (xdo_send_keysequence_window(_xdo, IntPtr.Zero, keySequence, 100000) != 0)
            throw new Exception("Key send failed");
    }
}