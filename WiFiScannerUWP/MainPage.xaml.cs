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
using Microsoft.Data.Sqlite.Internal; // needed for SqliteEngine.UseWinSqlite3() call

namespace WiFiScannerUWP
{
    public sealed partial class MainPage : Page
    {
        private WiFiAdapterScanner _wifiScanner;
        

        public MainPage()
        {
            InitializeComponent();

            SqliteEngine.UseWinSqlite3(); //Configuring library to use SDK version of SQLite

            _wifiScanner = new WiFiAdapterScanner();

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
                Create_WiFiScanner_db();

                await RunWifiScan();
            }
            catch (Exception ex)
            {
                MessageDialog md = new MessageDialog(ex.Message);

                await md.ShowAsync();
            }

            btnScan.IsEnabled = true;
        }

        public async Task RunWifiScan()
        {
            await _wifiScanner.ScanForNetworks();
            WiFiNetworkReport report = _wifiScanner.WiFiAdapter.NetworkReport;

            Geolocator geolocator = new Geolocator();
            Geoposition position = await geolocator.GetGeopositionAsync();

            var locationwifiGPSdata = new WiFiGPSDetail()
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
                Add_WiFiScanner_db(wifiSignal, locationwifiGPSdata);

            }
        }

        private async Task ShowMessage(string message)
        {
            var dialog = new MessageDialog(message);

            await dialog.ShowAsync();
        }

        private void venueName_TextChanged(object sender, TextChangedEventArgs e)
        { }

        private void Create_WiFiScanner_db()
        {
            using (SqliteConnection db = new SqliteConnection("Filename = WiFiScanner.db"))
            {
                try
                {
                    db.Open();
                }
                catch (SqliteException e)
                {
                    throw new Exception("SQL database not opened.");
                }

                //String drop_table_command = "DROP TABLE IF EXISTS WiFiSignals;";
                //SqliteCommand dropTable = new SqliteCommand(drop_table_command, db);
                //try
                //{
                //    dropTable.ExecuteNonQuery();
                //}
                //catch (SqliteException e)
                //{
                //    throw new Exception("SQL table not droped.");
                //}

                String create_table_command = "CREATE TABLE IF NOT EXISTS WiFiSignals (" +
                    //"BeaconInterval INTEGER," +
                    "Bssid TEXT," +
                    //"ChannelCenterFrequencyInKilohertz REAL," +
                    //"IsWiFiDirect INTEGER," +
                    //"NetworkKind TEXT," +
                    "NetworkRssiInDecibelMilliwatts REAL," +
                    //"PhyKind TEXT," +
                    //"SecuritySettings TEXT," +
                    //"SignalBars INTEGER," +
                    "Ssid TEXT," +
                    "Uptime INTEGER," +
                    "VenueName TEXT," +
                    "ScanTime TEXT, " +
                    "Latitude REAL, " +
                    "Longitude REAL, " +
                    "Accuracy REAL, " +
                    "TimeStamp TEXT " +
                    ")";
                SqliteCommand createTable = new SqliteCommand(create_table_command, db);
                try
                {
                    createTable.ExecuteNonQuery();
                }
                catch (SqliteException e)
                {
                    throw new Exception("SQL table not created.");
                }
                db.Close();
            }

        }

        private void Add_WiFiScanner_db(WiFiSignal wifiSignal, WiFiGPSDetail gpsSignal)
        {
            using (SqliteConnection db = new SqliteConnection("Filename = WiFiScanner.db"))
            {
                db.Open();

                using (SqliteCommand insertCommand = new SqliteCommand())
                {
                    insertCommand.Connection = db;
                    insertCommand.CommandText = "INSERT INTO WiFiSignals " +
                        "(" +
                        //"BeaconInterval, " +
                        "Bssid, " +
                        //"ChannelCenterFrequencyInKilohertz, " +
                        //"IsWiFiDirect, " +
                        //"NetworkKind, " +
                        "NetworkRssiInDecibelMilliwatts, " +
                        //"PhyKind, " +
                        //"SecuritySettings, " +
                        //"SignalBars, " +
                        "Ssid, " +
                        "Uptime, " +
                        "VenueName," +
                        "ScanTime," +
                        "Latitude," +
                        "Longitude," +
                        "Accuracy," +
                        "TimeStamp" +
                        ")" +
                        "VALUES " +
                        "(" +
                        //"@BeaconInterval," +
                        "@Bssid," +
                        //"@ChannelCenterFrequencyInKilohertz," +
                        //"@IsWiFiDirect," +
                        //"@NetworkKind," +
                        "@NetworkRssiInDecibelMilliwatts," +
                        //"@PhyKind," +
                        //"@SecuritySettings," +
                        //"@SignalBars," +
                        "@Ssid," +
                        "@Uptime," +
                        "@VenueName," +
                        "@ScanTime," +
                        "@Latitude," +
                        "@Longitude," +
                        "@Accuracy," +
                        "@TimeStamp" +
                        ")";
                    ////insertCommand.Parameters.AddWithValue("@BeaconInterval", wifiSignal.BeaconInterval.Ticks); //long INTEGER
                    insertCommand.Parameters.AddWithValue("@Bssid", wifiSignal.Bssid); //string TEXT
                    //insertCommand.Parameters.AddWithValue("@ChannelCenterFrequencyInKilohertz", wifiSignal.ChannelCenterFrequencyInKilohertz); //double REAL
                    //insertCommand.Parameters.AddWithValue("@IsWiFiDirect", (wifiSignal.IsWiFiDirect) ? 1 : 0); //bool INTEGER
                    //insertCommand.Parameters.AddWithValue("@NetworkKind", wifiSignal.NetworkKind); //string TEXT
                    insertCommand.Parameters.AddWithValue("@NetworkRssiInDecibelMilliwatts", wifiSignal.NetworkRssiInDecibelMilliwatts); //double REAL
                    //insertCommand.Parameters.AddWithValue("@PhyKind", wifiSignal.PhyKind); //string TEXT
                    //insertCommand.Parameters.AddWithValue("@SecuritySettings", wifiSignal.SecuritySettings); //string TEXT
                    //insertCommand.Parameters.AddWithValue("@SignalBars", wifiSignal.SignalBars); //byte INTEGER
                    insertCommand.Parameters.AddWithValue("@Ssid", wifiSignal.Ssid); //string TEXT
                    insertCommand.Parameters.AddWithValue("@Uptime", wifiSignal.Uptime.Ticks); //long INTEGER
                    insertCommand.Parameters.AddWithValue("@VenueName", wifiSignal.VenueName); //string TEXT
                    insertCommand.Parameters.AddWithValue("@ScanTime", wifiSignal.ScanTime.ToString("yyyy-MM-dd HH:mm:ss.fff")); //string TEXT
                    insertCommand.Parameters.AddWithValue("@Latitude", gpsSignal.Latitude); //double REAL
                    insertCommand.Parameters.AddWithValue("@Longitude", gpsSignal.Longitude); //double REAL
                    insertCommand.Parameters.AddWithValue("@Accuracy", gpsSignal.Accuracy); //double REAL
                    insertCommand.Parameters.AddWithValue("@TimeStamp", gpsSignal.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss.fff")); //string TEXT

                    try
                    {
                        insertCommand.ExecuteNonQuery();
                    }
                    catch (SqliteException e)
                    {
                        throw new Exception("SQL table INSERT not performed");
                    }
                }
                db.Close();
            }

            Output.ItemsSource = Grab_Entries();
        }

        private List<String> Grab_Entries()
        {
            List<String> entries = new List<string>();

            using (SqliteConnection db = new SqliteConnection("Filename = WiFiScanner.db"))
            {
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand("SELECT Ssid, Bssid, NetworkRssiInDecibelMilliwatts, TimeStamp, VenueName, Uptime FROM WiFiSignals ORDER BY Uptime DESC", db);
                SqliteDataReader query;

                try
                {
                    query = selectCommand.ExecuteReader();
                }
                catch (SqliteException e)
                {
                    //Handle error
                    return entries;
                }

                while (query.Read())
                {
                    TimeSpan interval = TimeSpan.FromTicks(query.GetInt64(5));
                    string up_time = interval.ToString("%d") + " day(s) " + interval.ToString(@"hh\:mm");
                    entries.Add(query.GetString(0) + " [MAC " + query.GetString(1) + "] " + query.GetString(2) + " dBm " + query.GetString(3) + " (" + query.GetString(4) + ") " + up_time);
                }

                db.Close();
            }
            return entries;
        }

    }
}