using QRCoder;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Windows.LocalWebRemote.Properties;
using WebRemote;

namespace Windows.LocalWebRemote
{
    /// <summary>
    /// Interaction logic for Window.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1(IPEndPoint endPoint)
        {
            InitializeComponent();
            LocalWebRemoteEndPoint = endPoint;
            StatusSvgUpdate();
        }

        public IPEndPoint LocalWebRemoteEndPoint { get { return field; }
            set
            {
                string localWebRemoteUrl = value.ToHttpUrl();

                IpLabel.Content = localWebRemoteUrl;
                var qrGen = new PayloadGenerator.Url(localWebRemoteUrl);
                var qrCode = new QRCodeGenerator().CreateQrCode(qrGen, QRCodeGenerator.ECCLevel.Q);
                using (var bitmapQRdata = new BitmapByteQRCode(qrCode))
                {
                    byte[] bitmapData = bitmapQRdata.GetGraphic(20);
                    using (var ms = new MemoryStream(bitmapData, 0, bitmapData.Length, writable: false, publiclyVisible: true))
                    {
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.StreamSource = ms;
                        bitmap.EndInit();
                        bitmap.Freeze();
                        ImageBox.Source = bitmap;
                        PortBox.Text = value.Port.ToString();
                    }
                }
                field = value;
            }
        }



        public static bool IsLocalWebRemoteRunning { get; set; }

        /// <summary>
        /// Occurs when a local web remote start or stop action is triggered, providing the new state as a nullable
        /// Boolean value.
        /// </summary>
        public EventHandler<bool?>? LocalWebRemoteStartStop;

        private void StartStopLocalWebRemote(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            LocalWebRemoteStartStop?.Invoke(null, null);
            StatusSvgUpdate();
        }

        public EventHandler<bool>? QuitApp;

        private void QuitLocalWebRemote(object sender, RoutedEventArgs e) 
        {
            QuitApp?.Invoke(null, true); 
            
        
        }
      



        internal static SearchValues<char> TheNumericChars = SearchValues.Create("0123456789");
        private void NumericTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            char x = e.Text[e.Text.Length - 1];
            e.Handled = !TheNumericChars.Contains(x); 
        }


        /// <summary>
        /// Updates main button and port box based on running status
        /// </summary>
        public void StatusSvgUpdate()
        {
            if (Window1.IsLocalWebRemoteRunning is true)
            {
                PathStatus.Data = Geometry.Parse("M292 5.655c-80.239 7.69-145.945 38.646-200.525 94.476C18.156 175.129-12.186 278.287 8.569 382 45.33 565.699 235.728 676.628 414.288 618.38c166.457-54.301 255.969-232.129 200.656-398.629C575.506 101.04 470.219 17.522 345.5 6.016c-10.053-.927-45.12-1.164-53.5-.361m-62.028 172.297c-25.982 3.463-48.227 24.196-53.522 49.884-2.507 12.158-1.679 179.416.926 187.164 7.687 22.868 24.969 39.335 46.639 44.442 9.31 2.194 174.857 2.292 185.155.11 21.707-4.601 40.576-22.501 47.49-45.052 1.728-5.635 1.84-11.441 1.84-95.5v-89.5l-2.739-8.078c-6.627-19.542-21.666-34.578-41.183-41.174l-8.078-2.73-85.5-.151c-47.025-.083-87.988.18-91.028.585M249 319v68h136V251H249v68");
                PortBox.IsEnabled = false;
                PortChangeSubmit.IsEnabled = false;
                StartHint.Text = UITexts.ClickToStop;
            }
            else
            {
                PathStatus.Data = Geometry.Parse("M225.748 9.457C64.142 29.943-33.628 196.283 27.644 346.5 69.099 448.135 173.841 510.231 282.5 497.589c156.27-18.18 255.001-173.742 205.057-323.089C458.847 88.651 382.454 24.568 292 10.456c-14.385-2.244-51.961-2.811-66.252-.999m-7.636 144.06c-17.365 4.874-31.086 18.766-35.581 36.025-1.849 7.098-2.297 116.122-.516 125.572 5.621 29.821 38.676 48.058 66.49 36.682 6.056-2.478 83.968-53.745 92.698-60.997 21.576-17.924 22.544-52.372 2.034-72.335-6.051-5.889-89.113-60.895-95.798-63.44-7.722-2.939-21.669-3.656-29.327-1.507M231 253.468c0 32.526.355 51.575.958 51.365.527-.183 18.021-11.807 38.876-25.831l37.918-25.498-36.126-24.077c-19.869-13.243-37.364-24.883-38.876-25.867l-2.75-1.79v51.698");
                PortBox.IsEnabled = true;
                PortChangeSubmit.IsEnabled = true;
                StartHint.Text = UITexts.ClickToStart;
            }
        }



        private void PortChangeSubmit_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (IsLocalWebRemoteRunning) return;

            bool parseResult = uint.TryParse(PortBox.Text, out uint portValue);
            if (parseResult is false || portValue is < 1 or > 65535)
            {
                System.Windows.MessageBox.Show("Please enter a valid port number(integer) between 1 and 65535.", "Invalid Port", MessageBoxButton.OK, MessageBoxImage.Information);
                PortBox.Text = LocalWebRemoteEndPoint.Port.ToString();
                return;
            }
            Settings.Default.Port = portValue;
            Settings.Default.Save();
            LocalWebRemoteEndPoint = new IPEndPoint(LocalWebRemoteEndPoint.Address, (int)portValue);
        }

        private void DonationButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://www.paypal.com/donate?hosted_button_id=WMDHKPRWZRXLW") { UseShellExecute = true });
            e.Handled = true;
        }
    }
}
