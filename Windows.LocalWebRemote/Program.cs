using System;
using System.Drawing;
using System.Windows.Forms;
using WebRemote;
using Microsoft.AspNetCore.Builder;
using System.IO;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Windows.LocalWebRemote
{
    class Program
    {
        static NotifyIcon _trayIcon = new();

        // lightweight async lock so we can await Start/Stop without blocking the UI thread
        static readonly SemaphoreSlim _webAppLock = new(1, 1);

        /// <summary>
        /// The web remote instance, statically created for use in conjuction with the tray icon and a window.
        /// </summary>
        internal protected static WebApplication _webApp = WebRemoteApplication.CreateLocalWebRemote(Settings.Default.Port);

        // Async start/stop to avoid blocking the UI thread
        internal protected static async Task StartLocalWebRemoteAsync()
        {
            await _webAppLock.WaitAsync();
            try
            {
                if (Window1.IsLocalWebRemoteRunning) return;
                _webApp = WebRemoteApplication.CreateLocalWebRemote(Settings.Default.Port);
                await _webApp.StartAsync();
               
            }
            finally
            {
                Window1.IsLocalWebRemoteRunning = true;
                _webAppLock.Release();
            }
        }

        internal protected static async Task StopLocalWebRemoteAsync()
        {
            await _webAppLock.WaitAsync();
            try
            {
                if (!Window1.IsLocalWebRemoteRunning) return;
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(0.01));
                try
                {
                    await _webApp.StopAsync(cts.Token);
                }
                catch (OperationCanceledException)
                {
                   Console.WriteLine("Web app stop timed out.");
                }
               
            }
            finally
            {
                Window1.IsLocalWebRemoteRunning = false;
                _webAppLock.Release();
            }
        }


        [STAThread]
        static void Main()
        {
            _trayIcon.Icon = SystemIcons.Error;
            _trayIcon.Visible = true;
            _trayIcon.Text = "Local WebRemote";

            // Optional: Add a context menu
            _trayIcon.ContextMenuStrip = new ContextMenuStrip();
            _trayIcon.ContextMenuStrip.Items.Add("Start WebRemote", null, async (s, e) => await StartLocalWebRemoteAsync());
            _trayIcon.ContextMenuStrip.Items.Add("Stop WebRemote", null, async (s, e) => await StopLocalWebRemoteAsync());
            _trayIcon.ContextMenuStrip.Items.Add("Settings", null, async (_, _) => await OpenSettingsAsync());
            _trayIcon.ContextMenuStrip.Items.Add("Exit", null, (s, e) => Application.Exit());

            // Ensure the web app is stopped when the application exits.
            Application.ApplicationExit += async (sender, args) =>
            {
                try {  await StopLocalWebRemoteAsync(); }
                finally
                {
                    _trayIcon.Visible = false;
                    _trayIcon.Dispose();
                }
            };
            OpenSettingsAsync().Wait();

            Application.Run();
            
        }


        internal static async Task OpenSettingsAsync()
        {
           Window1 settings = new Window1(WebRemoteApplication.GetDefaultEndPoint(Settings.Default.Port));
            // make this event handler async so Start/Stop won't block the UI thread
            settings.LocalWebRemoteStartStop += async (_, _) =>
            {
                if (Window1.IsLocalWebRemoteRunning)
                {
                    await StopLocalWebRemoteAsync(); Window1.IsLocalWebRemoteRunning = false;
                }
                else
                {
                    await StartLocalWebRemoteAsync(); Window1.IsLocalWebRemoteRunning = true;
                }
                settings.StatusSvgUpdate();
            };
            settings.ShowDialog();
        }
    }
}