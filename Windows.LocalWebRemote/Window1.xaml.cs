using Microsoft.AspNetCore.Http;
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
            StatusSvgUpdate();
        }



        public IPEndPoint LocalWebRemoteEndPoint { get { return field; }
            set
            {
                string localWebRemoteUrl;
                if (value.Port != 80) { localWebRemoteUrl = $"http://{value.Address}:{value.Port}/"; }
                else { localWebRemoteUrl = $"http://{value.Address}/"; }

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


        /// <summary>
        /// Updates main button and port box based on running status
        /// </summary>
        public void StatusSvgUpdate()
        {
            if (Window1.IsLocalWebRemoteRunning is true)
            {
                PathStatus.Data = Geometry.Parse("M270.5 38.585c-1.65.228-7.275.9-12.5 1.495-80.749 9.186-156.206 58.485-193.09 126.154-18.809 34.507-28.147 74.782-25.076 108.155C52.839 415.743 195.309 519.846 337.5 491.895c117.059-23.012 202.625-119.939 202.482-229.369C539.842 155.943 445.58 58.565 325 40.439c-9.905-1.488-47.805-2.778-54.5-1.854m-13 69.522c-60.951 6.409-120.13 35.379-166.095 81.308-14.571 14.559-17.093 18.788-16.157 27.092 1.118 9.92 8.588 16.462 19.706 17.258 10.295.736 12.43-.399 30.434-16.182 36.86-32.312 65.304-47.748 108.612-58.938 33.533-8.665 74.66-8.475 109.5.506 42.67 10.999 77.029 30.281 114.354 64.173 9.733 8.837 13.866 10.927 21.601 10.923 19.429-.011 29.131-17.08 18.172-31.968-8.346-11.337-35.742-34.958-56.627-48.824-36.568-24.28-76.627-39.237-120.301-44.92-12.846-1.671-49.043-1.916-63.199-.428m9 79.002c-45.883 4.275-94.07 27.541-129.12 62.343-9.007 8.943-10.252 10.612-11.321 15.178-3.237 13.829 7.302 25.947 22.441 25.803 8.659-.082 12.125-1.758 23-11.124 20.947-18.04 37.451-28.112 55.165-33.664 4.49-1.407 10.423-3.948 13.183-5.646 2.76-1.699 5.868-3.252 6.906-3.452 4.203-.809 12.058-3.501 17.246-5.91 6.233-2.893 7.92-3.019 28-2.088l14.5.672 19.5 6.807c37.596 13.124 50.091 19.782 76.093 40.546 14.558 11.626 17.089 13.134 23.432 13.966 16.835 2.206 31.068-13.612 25.1-27.895-4.516-10.809-41.427-40.009-66.396-52.525-39.241-19.67-76.154-26.885-117.729-23.011m21.5 77.547c-53.443 8.202-92.167 50.965-92.225 101.844-.064 56.764 39.75 100.176 93.725 102.195 54.267 2.03 100.903-29.358 112.408-75.656 11.876-47.791-1.495-91.441-34.472-112.537-18.755-11.997-56.045-19.436-79.436-15.846m8.5 15.028c-64.525 5.383-104.113 69.211-77.541 125.022 19.636 41.244 54.378 57.641 98.905 46.682 35.995-8.86 59.757-29.232 68.73-58.925 3.521-11.654 4.623-41.002 2.004-53.407-7.023-33.271-26.468-51.106-62.598-57.416-9.072-1.584-22.935-2.503-29.5-1.956m54.5 23.045c-2.15.819-24.747 19.564-36.426 30.217-5.459 4.98-10.185 9.054-10.502 9.054-.317 0-7.775-6.352-16.574-14.116C271.571 313.83 258.234 304 255.094 304c-3.25 0-6.096 2.963-6.056 6.307.048 4.118 3.013 7.427 19.462 21.715 7.15 6.211 15.449 13.665 18.441 16.564l5.442 5.271-9.442 9.848c-13.83 14.426-31.512 34.789-37.081 42.702-5.644 8.021-6.165 11.421-2.225 14.52 5.261 4.139 6.858 2.947 26.021-19.418 7.064-8.246 17.653-19.946 23.53-26l10.685-11.009 6.162 6c3.389 3.3 10.682 10.95 16.206 17 21.907 23.991 25.717 27.5 29.859 27.5 4.014 0 7.902-3.457 7.902-7.025 0-3.392-9.52-14.668-29.173-34.554l-19.843-20.079 11.758-10.497c6.467-5.773 16.258-14.221 21.758-18.772 15.124-12.516 15.867-13.86 10.577-19.15-2.824-2.824-4.924-3.394-8.077-2.194");
                PortBox.IsEnabled = false;
                PortChangeSubmit.IsEnabled = false;
            }
            else
            {
                PathStatus.Data = Geometry.Parse("M271 38.663C187.214 46.867 116.284 87.489 73.975 151.5c-24.314 36.786-37.742 85.281-34.059 123.002 3.673 37.616 18.643 79.116 38.956 107.998a7395.43 7395.43 0 0 0 7.073 10.032c20.48 28.986 55.051 57.994 89.877 75.415 44.675 22.347 89.981 31.979 132.224 28.109 102.963-9.431 191.377-75.943 220.368-165.777 28.367-87.904 1.904-170.031-75.582-234.564C406.28 56.945 329.695 32.916 271 38.663m-13.5 69.444c-60.951 6.409-120.13 35.379-166.095 81.308-14.571 14.559-17.093 18.788-16.157 27.092 1.118 9.92 8.588 16.462 19.706 17.258 10.295.736 12.43-.399 30.434-16.182 36.86-32.312 65.304-47.748 108.612-58.938 33.533-8.665 74.66-8.475 109.5.506 42.67 10.999 77.029 30.281 114.354 64.173 9.733 8.837 13.866 10.927 21.601 10.923 19.429-.011 29.131-17.08 18.172-31.968-8.346-11.337-35.742-34.958-56.627-48.824-36.568-24.28-76.627-39.237-120.301-44.92-12.846-1.671-49.043-1.916-63.199-.428m9 79.002c-45.883 4.275-94.07 27.541-129.12 62.343-9.007 8.943-10.252 10.612-11.321 15.178-3.237 13.829 7.302 25.947 22.441 25.803 8.718-.082 12.06-1.729 23.538-11.594 20.189-17.354 40.704-29.766 55.117-33.348 2.067-.514 2.804-1.459 3.331-4.27 2.604-13.88 14.829-24.485 29.841-25.887 8.25-.77 11.875.157 32.511 8.314 9.12 3.606 24.7 9.395 34.622 12.865 35.969 12.581 48.939 19.543 74.633 40.061 14.558 11.626 17.089 13.134 23.432 13.966 16.835 2.206 31.068-13.612 25.1-27.895-4.516-10.809-41.427-40.009-66.396-52.525-39.241-19.67-76.154-26.885-117.729-23.011m10 67.789c-8.622 2.206-13.904 7.686-15.495 16.077-1.338 7.054-1.302 121.025.04 126.843 1.83 7.936 7.351 13.142 15.991 15.078 8.557 1.918 10.295.998 57.357-30.346 63.378-42.209 61.716-40.784 62.436-53.526.805-14.245-1.972-16.779-43.329-39.538-64.149-35.302-67.8-36.942-77-34.588M220.258 288.5c-8.327 3.205-18.944 9.493-26.142 15.483-23.738 19.755-5.245 48.404 25.18 39.007 11.235-3.47 10.592-1.454 9.97-31.24-.376-17.976-.875-25.735-1.652-25.701-.613.027-3.923 1.13-7.356 2.451m65.197 10.757c-.265.692-.364 16.329-.219 34.75l.264 33.493 2.234.317c2.258.321 13.387-6.217 33.766-19.838 5.5-3.676 12.524-8.108 15.61-9.848 11.864-6.691 11.551-7.758-4.881-16.623-5.901-3.184-17.948-9.776-26.772-14.648-15.961-8.815-19.082-10.001-20.002-7.603");
                PortBox.IsEnabled = true;
                PortChangeSubmit.IsEnabled = true;
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
    }
}
