using System;
using System.Net;
using System.Security;
using System.Text.Json;
using Gdk;
using Gtk;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using WebRemote;

namespace Linux.LocalWebRemote
{

    class Program
    {

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
#pragma warning disable CS0612 // Type or member is obsolete


        private static StatusIcon trayIcon;

        private static Menu popupMenu = new Menu();

        internal protected static WebApplication _webApp {get; set;}

        internal static MainWindow? _mainWindow = null;

        #region local web remote management

        // lightweight async lock so we can await Start/Stop without blocking the UI thread
        internal static readonly SemaphoreSlim _webAppLock = new(1, 1);

        internal static bool IsLocalWebRemoteRunning { get; set; } = false;

        // Async start/stop to avoid blocking the UI thread
        internal protected static async Task StartLocalWebRemoteAsync()
        {
            await _webAppLock.WaitAsync();
            try
            {
                if (IsLocalWebRemoteRunning) return;
                LoadConfiguration();
                await _webApp.StartAsync();

            }
            finally
            {
                IsLocalWebRemoteRunning = true;
                _mainWindow?.SetStartStopButton();
                _webAppLock.Release();
            }
        }

        internal protected static async Task StopLocalWebRemoteAsync()
        {
            await _webAppLock.WaitAsync();
            try
            {
                if (IsLocalWebRemoteRunning is false) return;
                try
                {
                    await _webApp.StopAsync();
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("Web app stop timed out.");
                }

            }
            finally
            {
                IsLocalWebRemoteRunning = false;
                _mainWindow?.SetStartStopButton();
                _webAppLock.Release();
            }
        }

        #endregion

        static void Main()
        {
            Application.Init();
            LoadConfiguration();
            OpenMainWindow();

            trayIcon = new StatusIcon(new Pixbuf("webremote.png"));


            trayIcon.Activate += delegate { OpenMainWindow(); };
            trayIcon.PopupMenu += OnTrayIconPopup;

            ImageMenuItem startMenuItem = new ImageMenuItem("Start Web Remote Service") { Image = new Gtk.Image(Stock.MediaPlay, IconSize.Menu) };
            startMenuItem.Activated += async delegate { await Program.StartLocalWebRemoteAsync(); };

            ImageMenuItem stopMenuItem = new ImageMenuItem("Stop Web Remote Service") { Image = new Gtk.Image(Stock.MediaStop, IconSize.Menu) };
            stopMenuItem.Activated += async delegate { await Program.StopLocalWebRemoteAsync(); };

            ImageMenuItem menuItemOpenSettings = new ImageMenuItem("Open Settings") { Image = new Image(Stock.Properties, IconSize.Menu) };
            menuItemOpenSettings.Activated += delegate { OpenMainWindow(); };

            ImageMenuItem menuItemQuit = new ImageMenuItem("Quit") { Image = new Gtk.Image(Stock.Quit, IconSize.Menu) };
            menuItemQuit.Activated += delegate { StopLocalWebRemoteAsync().Wait(); Application.Quit();  };

            popupMenu.Append(startMenuItem);
            popupMenu.Append(stopMenuItem);
            popupMenu.Append(menuItemOpenSettings);
            popupMenu.Append(menuItemQuit);

            Application.Run();
        }


        internal static void OpenMainWindow()
        {
            if (_mainWindow is null)
            {
                _mainWindow = new MainWindow();
                _mainWindow.DeleteEvent += delegate { _mainWindow = null; };
            }
            _mainWindow.ShowAll();
        }

        // Create the popup menu, on right click.
        static void OnTrayIconPopup(object o, EventArgs args)
        {
            popupMenu.ShowAll();
            popupMenu.Popup();
        }


        internal static IPEndPoint _ipEndP;


        public static void LoadConfiguration()
        {
            if (File.Exists("appsettings.json") is false)
            {
                WriteConfig();
            }
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .Build();
            string? url = config.GetSection("Kestrel:Endpoints:MyHttpEndpoint:Url").Value;

            if (url is null) 
            { WriteConfig(); config.Reload(); }
            else 
            { _ipEndP = IPEndPoint.Parse(url.Remove(0,6).Replace("/", "")); }


            try { _webApp = WebRemoteApplication.CreateFromConfiguration(config); }
            catch (Exception ex)
            {
                Console.WriteLine("Error creating web application from configuration: " + ex.Message);

                WriteConfig();
                config.Reload();
                 _webApp = WebRemoteApplication.CreateFromConfiguration(config);
            }
        }




        public static void WriteConfig(uint port = 5031)
        {
            _ipEndP = WebRemoteApplication.GetDefaultEndPoint(port);
            var config = new
            {
                Kestrel = new
                {
                    Endpoints = new
                    {
                        MyHttpEndpoint = new
                        {
                            Url = _ipEndP.ToHttpUrl()
                        }
                    }
                }
            };
            string jsonString = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText("appsettings.json", jsonString);
        }

        public static void WriteConfig(string url)
        {
            _ipEndP = IPEndPoint.Parse(url.Remove(0,6).Replace("/", ""));
            var config = new
            {
                Kestrel = new
                {
                    Endpoints = new
                    {
                        MyHttpEndpoint = new
                        {
                            Url = _ipEndP.ToHttpUrl()
                        }
                    }
                }
            };
            string jsonString = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText("appsettings.json", jsonString);
        }
    }
}
