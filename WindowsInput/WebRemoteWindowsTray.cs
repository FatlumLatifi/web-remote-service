using System;
using System.Collections.Generic;
using System.Text;

namespace WindowsInputRemote
{
    public class WebRemoteWindowsTray
    {

        public static void InitializeTray(Action exitAction)
        {
            NotifyIcon trayIcon = new();
            trayIcon.Icon = SystemIcons.Application;
            trayIcon.Visible = true;
            trayIcon.Text = "Web Remote";
        }
    }
}
