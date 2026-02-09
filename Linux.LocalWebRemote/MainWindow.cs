using System;
using System.Net;
using System.IO;
using GLib;
using Gtk;
using Microsoft.AspNetCore.Builder;
using QRCoder;
using UI = Gtk.Builder.ObjectAttribute;
using System.Buffers;
using Microsoft.AspNetCore.Hosting;
using WebRemote;

namespace Linux.LocalWebRemote
{
    class MainWindow : Window
    {
        [UI] public Button? StartButton = default;
        [UI] private Button? QuitButton = default;
        [UI] private Button? SaveButton = default;

        [UI] private Entry? URLInput = default;
        [UI] private Entry? PortInput = default;

        [UI] private Image? QrImage = default;



    
        public MainWindow() : this(new Builder("MainWindow.glade")) { }

        private MainWindow(Builder builder) : base(builder.GetRawOwnedObject("MainWindow"))
        {
            builder.Autoconnect(this);
           LocalWebRemoteEndPoint = Program._ipEndP;
            DeleteEvent += delegate { this.Close();};
            if (Program.IsLocalWebRemoteRunning) { StartButton?.Image = new Image(Stock.MediaStop, IconSize.Button); }
            else { StartButton?.Image = new Image(Stock.MediaPlay, IconSize.Button); }


            QuitButton?.Clicked += delegate { Gtk.Application.Quit(); };
            SaveButton?.Clicked += delegate { Program.WriteConfig(uint.Parse(PortInput?.Text ?? "5031")); };

            PortInput?.KeyPressEvent += (o, args) =>
            {
                string text = PortInput?.Text ?? "";
                char x = text[text.Length - 1];
                if (TheNumericChars.Contains(x) is false)
                {
                   PortInput?.Text = text.Remove(text.Length - 1);
                   PortInput?.Position = PortInput.Text.Length;
                }
            };

            StartButton?.Label = "Clciked 0 times";
            StartButton?.Clicked += async delegate 
            {
                if (Program.IsLocalWebRemoteRunning)
                {
                    await Program.StopLocalWebRemoteAsync();
                    StartButton?.Label = $"Start";
                    StartButton?.Image = new Image(Stock.MediaPlay, IconSize.Button);
                }
                else
                {
                    await Program.StartLocalWebRemoteAsync();
                    StartButton?.Label = $"Stop";
                    StartButton?.Image = new Image(Stock.MediaStop, IconSize.Button);
                }
            };

            this.Shown += delegate { this.SetStartStopButton(); };
        }




        internal static SearchValues<char> TheNumericChars = SearchValues.Create("0123456789");


       

            public IPEndPoint LocalWebRemoteEndPoint
         {
            get { return field; }
            set
            {
                string localWebRemoteUrl;
                localWebRemoteUrl = value.ToHttpUrl();

                URLInput?.Text = localWebRemoteUrl;
                PortInput?.Text = value.Port.ToString();

                var qrGen = new PayloadGenerator.Url(localWebRemoteUrl);
                var qrCode = new QRCodeGenerator().CreateQrCode(qrGen, QRCodeGenerator.ECCLevel.Q);
                using (var bitmapQRdata = new BitmapByteQRCode(qrCode))
                {
                    byte[] bitmapData = bitmapQRdata.GetGraphic(20);
                    using (var ms = new MemoryStream(bitmapData, 0, bitmapData.Length, writable: false, publiclyVisible: true))
                    {
                        Gdk.Pixbuf pixbuf = new Gdk.Pixbuf(ms);
                        
                        
                        QrImage?.Pixbuf = pixbuf.ScaleSimple( 350, 350, Gdk.InterpType.Bilinear);
                    }
                }

                field = value;
            }
        }



        public void SetStartStopButton()
        {
              if (Program.IsLocalWebRemoteRunning is false)
                {
                   
                    StartButton?.Label = $"Start";
                    StartButton?.Image = new Image(Stock.MediaPlay, IconSize.Button);
                }
                else
                {
                    StartButton?.Label = $"Stop";
                    StartButton?.Image = new Image(Stock.MediaStop, IconSize.Button);
                }
        }
        
    }
}
