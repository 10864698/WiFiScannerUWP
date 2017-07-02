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
using Microsoft.Data.Sqlite;
using Microsoft.Data.Sqlite.Internal; // Not technically necessary, only needed for SqliteEngine.UseWinSqlite3() call

namespace WiFiScannerUWP
{
    public sealed partial class MainPage : Page
    {
        private WiFiAdapterScanner _wifiScanner;

        public MainPage()
        {
            InitializeComponent();

            _wifiScanner = new WiFiAdapterScanner();

            DataContext = _wifiScanner;

            SqliteEngine.UseWinSqlite3(); //Configuring library to use SDK version of SQLite
            using (SqliteConnection db = new SqliteConnection("Filename = WiFiScanner.db"))
            {
                db.Open();
                String tableCommand = "CREATE TABLE IF NOT EXISTS WiFiSignals (" +
                    "Primary_Key INTEGER PRIMARY KEY AUTOINCREMENT," +
                    "BeaconInterval TIME NULL," +
                    "Bssid VARCHAR(255) NULL," +
                    "ChannelCenterFrequencyInKilohertz INT NULL," +
                    "IsWiFiDirect BIT NULL,NetworkKind VARCHAR(255) NULL," +
                    "NetworkKind VARCHAR(255) NUL," +
                    "NetworkRssiInDecibelMilliwatts DOUBLE NULL," +
                    "PhyKind VARCHAR(255) NULL," +
                    "SecuritySettings VARCHAR(255) NULL," +
                    "SignalBars INT NULL," +
                    "Ssid VARCHAR(255) NULL," +
                    "Uptime DATETIME NULL," +
                    "VenueName VARCHAR(255) NULL," +
                    "ScanTime VARCHAR(255) NULL)";
                SqliteCommand createTable = new SqliteCommand(tableCommand, db);
                try
                {
                    createTable.ExecuteReader();
                }
                catch (SqliteException e)
                {
                    //throw new Exception("SQL table not created.");
                }
                db.Close();
            }
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
                txbReport.Text = networkInfo.ToString();
            }

                //try
                //{
                //    StringBuilder networkInfo = await RunWifiScan();
                //    List<string> wifidata = new List<string>() { networkInfo.ToString() };

                //    //Roaming copy
                //    ApplicationDataContainer roamingSettings = ApplicationData.Current.RoamingSettings;
                //    StorageFolder roamingFolder = ApplicationData.Current.RoamingFolder;

                //    //Clear all data
                //    //await ApplicationData.Current.ClearAsync(ApplicationDataLocality.Roaming);

                //    StorageFile roamingFile = await roamingFolder.CreateFileAsync("wifidata.csv", CreationCollisionOption.OpenIfExists);
                //    roamingFile = await roamingFolder.GetFileAsync("wifidata.csv");
                //    await FileIO.AppendLinesAsync(roamingFile, wifidata);

                //    //GUI Output
                //    txbReport.Text = networkInfo.ToString();/*await FileIO.ReadTextAsync(roamingFile);*/

                //    //Local copy in Documents folder
                //    StorageFolder storageFolder = KnownFolders.DocumentsLibrary;

                //    StorageFile storageFile = await storageFolder.CreateFileAsync("wifidata.csv", CreationCollisionOption.ReplaceExisting);
                //    storageFile = await storageFolder.GetFileAsync("wifidata.csv");

                //    await FileIO.WriteTextAsync(storageFile, await FileIO.ReadTextAsync(roamingFile));
                //}
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

            var locationwifiGPSdata = new LocationWiFiGPSDetail()
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
                    BeaconInterval = availableNetwork.BeaconInterval,
                    Bssid = availableNetwork.Bssid,
                    ChannelCenterFrequencyInKilohertz = availableNetwork.ChannelCenterFrequencyInKilohertz,
                    IsWiFiDirect = availableNetwork.IsWiFiDirect,
                    NetworkKind = availableNetwork.NetworkKind.ToString(),
                    NetworkRssiInDecibelMilliwatts = availableNetwork.NetworkRssiInDecibelMilliwatts,
                    PhyKind = availableNetwork.PhyKind.ToString(),
                    SecuritySettings = availableNetwork.SecuritySettings.NetworkEncryptionType.ToString(),
                    SignalBars = availableNetwork.SignalBars,
                    Ssid = availableNetwork.Ssid,
                    Uptime = availableNetwork.Uptime,
                    VenueName = _wifiScanner.venueName,
                    ScanTime = _wifiScanner.scanTime
            };

                locationwifiGPSdata.WiFiSignals.Add(wifiSignal);
                Add_WiFiScanner_db(wifiSignal);

            }

            StringBuilder networkInfo = CreateCsvReport(locationwifiGPSdata);

            return networkInfo;
        }

        private void Add_WiFiScanner_db(WiFiSignal wifiSignal)
        {
            using (SqliteConnection db = new SqliteConnection("Filename = WiFiScanner.db"))
            {
                db.Open();

                SqliteCommand insertCommand = new SqliteCommand();
                insertCommand.Connection = db;

                //Use parameterized query to prevent SQL injection attacks
                insertCommand.CommandText = "INSERT INTO MyTable VALUES (" +
                    "NULL," +
                    "@BeaconInterval," +
                    "@Bssid," +
                    "@ChannelCenterFrequencyInKilohertz," +
                    "@IsWiFiDirect," +
                    "@NetworkKind," +
                    "@NetworkRssiInDecibelMilliwatts," +
                    "@PhyKind," +
                    "@SecuritySettings," +
                    "@SignalBars," +
                    "@Ssid," +
                    "@Uptime," +
                    "@VenueName" +
                    "@ScanTime;)";
                insertCommand.Parameters.AddWithValue("@BeaconInterval", wifiSignal.BeaconInterval);
                insertCommand.Parameters.AddWithValue("@Bssid", wifiSignal.Bssid);
                insertCommand.Parameters.AddWithValue("@ChannelCenterFrequencyInKilohertz", wifiSignal.ChannelCenterFrequencyInKilohertz);
                insertCommand.Parameters.AddWithValue("@IsWiFiDirect", wifiSignal.IsWiFiDirect);
                insertCommand.Parameters.AddWithValue("@NetworkKind", wifiSignal.NetworkKind);
                insertCommand.Parameters.AddWithValue("@NetworkRssiInDecibelMilliwatts", wifiSignal.NetworkRssiInDecibelMilliwatts);
                insertCommand.Parameters.AddWithValue("@PhyKind", wifiSignal.PhyKind);
                insertCommand.Parameters.AddWithValue("@SecuritySettings", wifiSignal.SecuritySettings);
                insertCommand.Parameters.AddWithValue("@SignalBars", wifiSignal.SignalBars);
                insertCommand.Parameters.AddWithValue("@Ssid", wifiSignal.Ssid);
                insertCommand.Parameters.AddWithValue("@Uptime", wifiSignal.Uptime);
                insertCommand.Parameters.AddWithValue("@VenueName", wifiSignal.VenueName);
                insertCommand.Parameters.AddWithValue("@ScanTime", wifiSignal.ScanTime);

                try
                {
                    insertCommand.ExecuteReader();
                }
                catch (SqliteException e)
                {
                    //throw new Exception("SQL table INSERT not performed");
                }
                db.Close();
            }
        }

        private StringBuilder CreateCsvReport(LocationWiFiGPSDetail wifiPoint)
        {

            StringBuilder networkInfo = new StringBuilder();

            networkInfo.AppendLine("MAC,SSID,SignalBars,Type,Lat,Long,Accuracy,Encryption,Venue,Time");
           
            foreach (var wifiSignal in wifiPoint.WiFiSignals)
            {
                networkInfo.Append($"{wifiSignal.Bssid},");
                networkInfo.Append($"{wifiSignal.Ssid},");
                networkInfo.Append($"{wifiSignal.SignalBars},");
                networkInfo.Append($"{wifiSignal.NetworkKind},");
                networkInfo.Append($"{wifiPoint.Latitude},");
                networkInfo.Append($"{wifiPoint.Longitude},");
                networkInfo.Append($"{wifiPoint.Accuracy},");
                networkInfo.Append($"{wifiSignal.SecuritySettings},");
                networkInfo.Append($"{_wifiScanner.venueName},");
                networkInfo.Append($"{_wifiScanner.scanTime.ToString("G")}");
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