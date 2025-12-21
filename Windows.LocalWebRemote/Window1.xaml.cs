using QRCoder;
using System.Buffers;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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

            string localWebRemoteUrl;
            if (endPoint.Port != 80) { localWebRemoteUrl = $"http://{endPoint.Address}:{endPoint.Port}/"; }
            else  { localWebRemoteUrl = $"http://{endPoint.Address}/"; }

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
                    PortBox.Text = endPoint.Port.ToString();
                }
            }
            StatusSvgUpdate();
        }
        public IPEndPoint LocalWebRemoteEndPoint;
        public static bool IsLocalWebRemoteRunning { get; set; }
        public EventHandler<bool?>? LocalWebRemoteStartStop;
        private void SvgRunningStatusButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            LocalWebRemoteStartStop?.Invoke(null, null);

            StatusSvgUpdate();
        }

        internal static SearchValues<char> TheNumericChars = SearchValues.Create("0123456789");
        private void NumericTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            char x = e.Text[e.Text.Length - 1];
            e.Handled = !TheNumericChars.Contains(x); 
        }  

        public void StatusSvgUpdate()
        {
            if (Window1.IsLocalWebRemoteRunning is true)
            {
                PathStatus.Data = Geometry.Parse("M8.46447 15.5355C6.51184 13.5829 6.51184 10.4171 8.46447 8.46447M12 7C13.2796 7 14.5592 7.48816 15.5355 8.46447C16.5118 9.44078 17 10.7204 17 12M5.63604 18.364C2.12132 14.8492 2.12132 9.15076 5.63604 5.63604M8.64587 3.64587C11.8893 2.34541 15.7367 3.0088 18.364 5.63604C20.9912 8.26328 21.6546 12.1107 20.3541 15.3541M4 3.99992L11.2929 11.2929M20 19.9999L12.7071 12.7071M11.2929 11.2929C11.1119 11.4738 11 11.7238 11 12C11 12.5523 11.4477 13 12 13C12.2762 13 12.5262 12.8881 12.7071 12.7071M11.2929 11.2929L12.7071 12.7071");

            }
            else
            {
                PathStatus.Data = Geometry.Parse("M15.5355 15.5355C17.4882 13.5829 17.4882 10.4171 15.5355 8.46447C13.5829 6.51184 10.4171 6.51184 8.46447 8.46447C6.51184 10.4171 6.51184 13.5829 8.46447 15.5355M18.364 18.364C21.8787 14.8492 21.8787 9.15076 18.364 5.63604C14.8492 2.12132 9.15076 2.12132 5.63604 5.63604C2.12132 9.15076 2.12132 14.8492 5.63604 18.364M13 12C13 12.5523 12.5523 13 12 13C11.4477 13 11 12.5523 11 12C11 11.4477 11.4477 11 12 11C12.5523 11 13 11.4477 13 12Z");
            }
        }
    }
}
