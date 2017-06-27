using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Devices.WiFi;
using Windows.UI.Popups;
using System.Threading.Tasks;
using System.Text;
using Windows.Devices.Geolocation;
using Windows.Storage;

namespace WiFiScannerUWP
{
    public sealed partial class MainPage : Page
    {
        private WiFiScanner _wifiScanner;

        public MainPage()
        {
            InitializeComponent();

            _wifiScanner = new WiFiScanner();

            DataContext = _wifiScanner;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await InitializeScanner();
        }

        private async Task InitializeScanner()
        {
            await _wifiScanner.InitializeScanner();
        }

        private async void btnScan_Click(object sender, RoutedEventArgs e)
        {
            btnScan.IsEnabled = false;

            try
            {
                StringBuilder networkInfo = await RunWifiScan();
                List<string> wifidata = new List<string>() { networkInfo.ToString() };

                //Roaming copy
                ApplicationDataContainer roamingSettings = ApplicationData.Current.RoamingSettings;
                StorageFolder roamingFolder = ApplicationData.Current.RoamingFolder;

                StorageFile roamingFile = await roamingFolder.CreateFileAsync("wifidata.csv", CreationCollisionOption.OpenIfExists);
                roamingFile = await roamingFolder.GetFileAsync("wifidata.csv");
                await FileIO.AppendLinesAsync(roamingFile, wifidata);

                //GUI Output
                txbReport.Text = await FileIO.ReadTextAsync(roamingFile);

                //Local copy in Documents folder
                StorageFolder storageFolder = KnownFolders.DocumentsLibrary;

                StorageFile storageFile = await storageFolder.CreateFileAsync("wifidata.csv", CreationCollisionOption.ReplaceExisting);
                storageFile = await storageFolder.GetFileAsync("wifidata.csv");

                await FileIO.WriteTextAsync(storageFile, await FileIO.ReadTextAsync(roamingFile));
            }
            catch (Exception ex)
            {
                MessageDialog md = new MessageDialog(ex.Message);

                await md.ShowAsync();
            }

            btnScan.IsEnabled = true;
        }

        private async Task<StringBuilder> RunWifiScan()
        {
            await _wifiScanner.ScanForNetworks();

            Geolocator geolocator = new Geolocator();

            Geoposition position = await geolocator.GetGeopositionAsync();

            WiFiNetworkReport report = _wifiScanner.WiFiAdapter.NetworkReport;

            var wifiPoint = new WiFiPointData()
            {
                Latitude = position.Coordinate.Point.Position.Latitude,
                Longitude = position.Coordinate.Point.Position.Longitude,
                Accuracy = position.Coordinate.Accuracy,
                TimeStamp = position.Coordinate.Timestamp
            };

            foreach (var availableNetwork in report.AvailableNetworks)
            {
                WiFiSignal wifiSignal = new WiFiSignal()
                {
                    MacAddress = availableNetwork.Bssid,
                    Ssid = availableNetwork.Ssid,
                    SignalBars = availableNetwork.SignalBars,
                    ChannelCenterFrequencyInKilohertz = availableNetwork.ChannelCenterFrequencyInKilohertz,
                    NetworkKind = availableNetwork.NetworkKind.ToString(),
                    PhysicalKind = availableNetwork.PhyKind.ToString(),
                    Encryption = availableNetwork.SecuritySettings.NetworkEncryptionType.ToString(),
                    VenueName = _wifiScanner.venueName,
                    StartTime = _wifiScanner.startTime
            };

                wifiPoint.WiFiSignals.Add(wifiSignal);
            }

            StringBuilder networkInfo = CreateCsvReport(wifiPoint);

            return networkInfo;
        }

        private StringBuilder CreateCsvReport(WiFiPointData wifiPoint)
        {

            StringBuilder networkInfo = new StringBuilder();

            networkInfo.AppendLine("MAC,SSID,SignalBars,Type,Lat,Long,Accuracy,Encryption,Venue,Time");
            
            foreach (var wifiSignal in wifiPoint.WiFiSignals)
            {
                networkInfo.Append($"{wifiSignal.MacAddress},");
                networkInfo.Append($"{wifiSignal.Ssid},");
                networkInfo.Append($"{wifiSignal.SignalBars},");
                networkInfo.Append($"{wifiSignal.NetworkKind},");
                networkInfo.Append($"{wifiPoint.Latitude},");
                networkInfo.Append($"{wifiPoint.Longitude},");
                networkInfo.Append($"{wifiPoint.Accuracy},");
                networkInfo.Append($"{wifiSignal.Encryption},");
                networkInfo.Append($"{_wifiScanner.venueName},");
                networkInfo.Append($"{_wifiScanner.startTime.ToString("G")}");
                networkInfo.AppendLine();
            }

            return networkInfo;
        }

        private async Task ShowMessage(string message)
        {
            var dialog = new MessageDialog(message);

            await dialog.ShowAsync();
        }

        private void venueName_TextChanged(object sender, TextChangedEventArgs e)
        { }
    }
}