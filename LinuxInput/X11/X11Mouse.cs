using System.Runtime.InteropServices;
using WebRemote.Models;

namespace LinuxInput.X11;

    public class X11Mouse : IWebRemoteMouse
    {
        private IntPtr _xdo;

        public X11Mouse(IntPtr xdo)
        {
            _xdo = xdo;
        }

        public void MoveBy(int x, int y) => xdo_move_mouse_relative(_xdo, x, y);
        [DllImport("libxdo")]
        private static extern int xdo_move_mouse_relative(IntPtr xdo, int x, int y);


        /// <summary>
        ///  Sends mouse button
        /// </summary>
        /// <param name="button">The mouse button. Generally, 1 is left, 2 is middle, 3 is right, 4 is wheel up, 5 is wheel down.</param>
        public void SendClick(int button) => xdo_click_window(_xdo, IntPtr.Zero, button); 
        /// <summary>
        /// Send a click for a specific mouse button at the current mouse location.  
        /// </summary>
        /// <param name="xdo">xdo pointer</param>
        /// <param name="window">The window you want to send the event to or CURRENTWINDOW</param>
        /// <param name="button">button The mouse button. Generally, 1 is left, 2 is middle, 3 is right, 4 is wheel up, 5 is wheel down.</param>
        /// <returns></returns>
        [DllImport("libxdo")]
        public static extern int xdo_click_window(IntPtr xdo, IntPtr window, int button);

            

    }

