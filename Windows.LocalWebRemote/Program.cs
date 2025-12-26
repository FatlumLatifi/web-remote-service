using WebRemote;
using Microsoft.AspNetCore.Builder;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace Windows.LocalWebRemote
{
    class Program
    {
        static NotifyIcon _trayIcon = new();

        static Window1? _settingsWindow;

        #region local web remote management

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
                _settingsWindow?.StatusSvgUpdate();
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
                _settingsWindow?.StatusSvgUpdate();
                _webAppLock.Release();
            }
        }

        #endregion


        [STAThread]
        static void Main()
        {
            // CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("de-DE"); // deutsche Sprache erzwingen zum Testen

            _trayIcon.Icon = new Icon(_pathToIco);
            _trayIcon.Visible = true;
            _trayIcon.Text = "Local WebRemote";

            _trayIcon.ContextMenuStrip = new ContextMenuStrip();
            _trayIcon.ContextMenuStrip.Items.Add("Start WebRemote", null, async (s, e) => await StartLocalWebRemoteAsync());
            _trayIcon.ContextMenuStrip.Items.Add("Stop WebRemote", null, async (s, e) => await StopLocalWebRemoteAsync());
            _trayIcon.ContextMenuStrip.Items.Add("Settings", null, async (_, _) => await OpenSettingsAsync());
            _trayIcon.ContextMenuStrip.Items.Add("Exit", null, (s, e) => Application.Exit());

            // Ensures the web app is stopped when the application exits.
            Application.ApplicationExit += async (sender, args) =>
            {
                try { await StopLocalWebRemoteAsync(); }
                finally
                {
                    _trayIcon.Visible = false;
                    _trayIcon.Dispose();
                }
            };
            OpenSettingsAsync().Wait();
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
            Application.Run();
        }



        internal static async Task OpenSettingsAsync()
        {
            if (_settingsWindow != null)
            {
                _settingsWindow.StatusSvgUpdate();
                _settingsWindow.Close();
                _settingsWindow = null;
            }

            _settingsWindow = new Window1(WebRemoteApplication.GetDefaultEndPoint(Settings.Default.Port));
            //  this event handler is async so Start/Stop won't block the UI thread
            _settingsWindow.LocalWebRemoteStartStop += async (_, _) =>
            {
                if (Window1.IsLocalWebRemoteRunning)
                {
                    await StopLocalWebRemoteAsync(); Window1.IsLocalWebRemoteRunning = false;
                }
                else
                {
                    await StartLocalWebRemoteAsync(); Window1.IsLocalWebRemoteRunning = true;
                }
                _settingsWindow.StatusSvgUpdate();
            };

            _settingsWindow.QuitApp += async (_, _) =>
            {
                await StopLocalWebRemoteAsync();
                Application.Exit();
                Process.GetCurrentProcess().Kill(); // Das ist nötig, weil Application.Exit() den app nicht beendet in dem ersten window.
            };
            _settingsWindow.ShowDialog();
        }


        internal static string _pathToIco
        {
            get
            {
               return Path.Combine(Application.StartupPath, "wwwroot", "favicon.ico");
            }
        }
    }
}