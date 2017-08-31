using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Devices.WiFi;
using Windows.UI.Popups;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Storage;
using Microsoft.Data.Sqlite;
using Microsoft.Data.Sqlite.Internal; // needed for SqliteEngine.UseWinSqlite3() call
using System.ComponentModel;

namespace WiFiScannerUWP
{
    public partial class MainPage : Page, INotifyPropertyChanged
    {
        ////Set the API Endpoint to Graph 'me' endpoint
        //string _graphAPIEndpoint = "https://graph.microsoft.com/v1.0/me";

        ////Set the scope for API call to user.read
        //string[] _scopes = new string[] { "user.read" };
        private WifiAdapterScanner _wifiScanner;
        private string _venueName = "No Name Entered";

        public MainPage()
        {
            InitializeComponent();

            SqliteEngine.UseWinSqlite3(); //Configuring library to use SDK version of SQLite

            _wifiScanner = new WifiAdapterScanner();

            DataContext = _wifiScanner;
        }

        private async void PageLoaded(object sender, RoutedEventArgs e)
        {
            await InitializeScanner();
        }

        private async Task InitializeScanner()
        {
            await _wifiScanner.InitializeScanner();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void VenueNameTextChanged(object sender, TextChangedEventArgs e)
        {
            if (venueNameTextBox.Text == "")
            {
                _venueName = "No Name Entered";
            }

            else
            {
                _venueName = venueNameTextBox.Text;
            }
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public string VenueName
        {
            get
            {
                return _venueName;
            }
            set
            {
                if (venueNameTextBox.Text == "")
                {
                    _venueName = "No Name Entered";
                    OnPropertyChanged("venueNameTextBox");
                }

                else
                {
                    _venueName = venueNameTextBox.Text;
                    OnPropertyChanged("venueNameTextBox");
                }
            }
        }

        private async void ClearTableButtonClick(object sender, RoutedEventArgs e)
        {
            ButtonClearTable.IsEnabled = false;

            try
            {
                ClearWifiSignalsTable(VenueName);
            }
            catch (Exception ex)
            {
                MessageDialog md = new MessageDialog(ex.Message);

                await md.ShowAsync();
            }

            ButtonClearTable.IsEnabled = true;
        }

        private async void ClearDatabaseButtonClick(object sender, RoutedEventArgs e)
        {
            ButtonClearDatabase.IsEnabled = false;

            try
            {
                ClearDatabase();
            }
            catch (Exception ex)
            {
                MessageDialog md = new MessageDialog(ex.Message);

                await md.ShowAsync();
            }

            ButtonClearDatabase.IsEnabled = true;
        }

        private async void ScanButtonClick(object sender, RoutedEventArgs e)
        {
            ButtonScan.IsEnabled = false;

            try
            {
                await RunWifiScan();
            }
            catch (Exception ex)
            {
                MessageDialog md = new MessageDialog(ex.Message);

                await md.ShowAsync();
            }

            ButtonScan.IsEnabled = true;
        }

        private async void ShowButtonClick(object sender, RoutedEventArgs e)
        {
            ButtonShow.IsEnabled = false;

            try
            {
                Output.ItemsSource = ReadWifiScannerDatabase;
            }
            catch (Exception ex)
            {
                MessageDialog md = new MessageDialog(ex.Message);

                await md.ShowAsync();
            }

            ButtonShow.IsEnabled = true;
        }

        private async Task RunWifiScan()
        {
            await _wifiScanner.ScanForNetworks();
            WiFiNetworkReport report = _wifiScanner.WiFiAdapter.NetworkReport;

            Geolocator geolocator = new Geolocator()
            {
                DesiredAccuracy = PositionAccuracy.High
            };
            Geoposition position = await geolocator.GetGeopositionAsync();

            var locationWifiGpsData = new WifiGpsDetail()
            {
                Accuracy = position.Coordinate.Accuracy,
                Altitude = position.Coordinate.Point.Position.Altitude,
                LocationStatus = geolocator.LocationStatus,
                Latitude = position.Coordinate.Point.Position.Latitude,
                Longitude = position.Coordinate.Point.Position.Longitude,
                TimeStamp = position.Coordinate.Timestamp
            };

            foreach (var availableNetwork in report.AvailableNetworks)
            {
                WifiSignal wifiSignal = new WifiSignal()
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
                    VenueName = VenueName,
                    ScanTime = _wifiScanner.scanTime
                };

                AddWifiScanResultsToWifiScannerDatabase(wifiSignal, locationWifiGpsData);
            }
        }

        private async Task ShowMessage(string message)
        {
            var dialog = new MessageDialog(message);

            await dialog.ShowAsync();
        }

        private void CreateVenueTableInWifiScannerDatabaseIfNotExists(string tableName)
        {
            using (SqliteConnection database = new SqliteConnection("Filename = WiFiScanner.db"))
            {
                try
                {
                    database.Open();
                }
                catch (SqliteException e)
                {
                    throw new Exception("SQL database not opened.");
                }

                String sqlCreateTableCommand = "CREATE TABLE IF NOT EXISTS " + RemoveWhiteSpace(tableName) + " (" +
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
                    "ScanCount INTEGER, " +
                    "VenueName TEXT," +
                    "ScanTime TEXT, " +
                    "Accuracy REAL, " +
                    "Altitude REAL," +
                    "LocationStatus TEXT," +
                    "Latitude REAL, " +
                    "Longitude REAL, " +
                    "TimeStamp TEXT " +
                    ")";

                SqliteCommand createTable = new SqliteCommand(sqlCreateTableCommand, database);
                try
                {
                    createTable.ExecuteNonQuery();
                }
                catch (SqliteException e)
                {
                    throw new Exception("SQL table " + RemoveWhiteSpace(tableName) + " not created.");
                }
                database.Close(); database.Dispose();
            }

            Output.ItemsSource = ReadWifiScannerDatabase;
        }

        private string RemoveWhiteSpace(string input)
        {
            int j = 0, inputlen = input.Length;
            char[] newarr = new char[inputlen];

            for (int i = 0; i < inputlen; ++i)
            {
                char tmp = input[i];

                if (!char.IsWhiteSpace(tmp))
                {
                    newarr[j] = tmp;
                    ++j;
                }
            }
            return new String(newarr, 0, j);
        }

        private void ClearWifiSignalsTable(string tableName)
        {
            using (SqliteConnection database = new SqliteConnection("Filename = WiFiScanner.db"))
            {
                try
                {
                    database.Open();
                }
                catch (SqliteException e)
                {
                    throw new Exception("SQL database not opened.");
                }

                String sqlDropTableCommand = "DROP TABLE IF EXISTS " + RemoveWhiteSpace(tableName) + ";";
                SqliteCommand dropTable = new SqliteCommand(sqlDropTableCommand, database);
                try
                {
                    dropTable.ExecuteNonQuery();
                }
                catch (SqliteException e)
                {
                    throw new Exception("SQL table not dropped.");
                }

                String sqlCreateTableCommand = "CREATE TABLE IF NOT EXISTS " + RemoveWhiteSpace(tableName) + " (" +
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
                    "ScanCount INTEGER, " +
                    "VenueName TEXT," +
                    "ScanTime TEXT, " +
                    "Accuracy REAL, " +
                    "Altitude REAL," +
                    "LocationStatus TEXT," +
                    "Latitude REAL, " +
                    "Longitude REAL, " +
                    "TimeStamp TEXT " +
                    ")";
                SqliteCommand createTable = new SqliteCommand(sqlCreateTableCommand, database);
                try
                {
                    createTable.ExecuteNonQuery();
                }
                catch (SqliteException e)
                {
                    throw new Exception("SQL table not created.");
                }
                database.Close(); database.Dispose();
            }

            Output.ItemsSource = ReadWifiScannerDatabase;

        }

        private void ClearDatabase()
        {
            List<String> tableNames = new List<string>();

            using (SqliteConnection database = new SqliteConnection("Filename = WiFiScanner.db"))
            {
                database.Open();

                SqliteCommand sqlSelectCommand = new SqliteCommand(
                    "SELECT name FROM sqlite_master WHERE type = 'table';", database);
                SqliteDataReader query;

                try
                {
                    query = sqlSelectCommand.ExecuteReader();
                }
                catch (SqliteException e)
                {
                    throw new Exception("SQL database no tables");
                }

                while (query.Read())
                {
                    tableNames.Add(query.GetString(0));
                }

                database.Close(); database.Dispose();
            }

            using (SqliteConnection database = new SqliteConnection("Filename = WiFiScanner.db"))
            {
                try
                {
                    database.Open();
                }
                catch (SqliteException e)
                {
                    throw new Exception("SQL database not opened.");
                }

                foreach (var tableName in tableNames)
                {
                    String sqlDropTableCommand = "DROP TABLE IF EXISTS " + RemoveWhiteSpace(tableName) + ";";
                    SqliteCommand dropTable = new SqliteCommand(sqlDropTableCommand, database);
                    try
                    {
                        dropTable.ExecuteNonQuery();
                    }
                    catch (SqliteException e)
                    {
                        throw new Exception("SQL table not dropped.");
                    }
                }

                database.Close(); database.Dispose();
            }

            Output.ItemsSource = tableNames;
        }

        private void AddWifiScanResultsToWifiScannerDatabase(WifiSignal wifiSignal, WifiGpsDetail gpsSignal)
        {
            using (SqliteConnection database = new SqliteConnection("Filename = WiFiScanner.db"))
            {
                database.Open();

                //create if table doesn't exist
                CreateVenueTableInWifiScannerDatabaseIfNotExists(RemoveWhiteSpace(VenueName));

                //check if VenueName / Bssid / Ssid exists already
                using (SqliteCommand sqlCheckExistingWifiSignalCommand = new SqliteCommand())
                {
                    sqlCheckExistingWifiSignalCommand.Connection = database;
                    sqlCheckExistingWifiSignalCommand.CommandText = "SELECT count(*) FROM " + RemoveWhiteSpace(VenueName) + " " +
                        "WHERE VenueName = @VenueName " +
                        "AND Bssid = @Bssid " +
                        "AND Ssid = @Ssid";
                    sqlCheckExistingWifiSignalCommand.Parameters.AddWithValue("@VenueName", wifiSignal.VenueName); //string TEXT
                    sqlCheckExistingWifiSignalCommand.Parameters.AddWithValue("@Bssid", wifiSignal.Bssid); //string TEXT
                    sqlCheckExistingWifiSignalCommand.Parameters.AddWithValue("@Ssid", wifiSignal.Ssid); //string TEXT
                    int count = Convert.ToInt32(sqlCheckExistingWifiSignalCommand.ExecuteScalar()); //check for existing record
                    if (count == 0)
                    {
                        using (SqliteCommand insertCommand = new SqliteCommand())
                        {
                            insertCommand.Connection = database;
                            insertCommand.CommandText = "INSERT INTO " + RemoveWhiteSpace(VenueName) + " " +
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
                                "ScanCount, " +
                                "VenueName, " +
                                "ScanTime, " +
                                "Accuracy, " +
                                "Altitude, " +
                                "LocationStatus, " +
                                "Latitude, " +
                                "Longitude, " +
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
                                "@ScanCount, " +
                                "@VenueName," +
                                "@ScanTime," +
                                "@Accuracy," +
                                "@Altitude," +
                                "@LocationStatus," +
                                "@Latitude," +
                                "@Longitude," +
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
                            insertCommand.Parameters.AddWithValue("@ScanCount", (long)count); //long INTEGER
                            insertCommand.Parameters.AddWithValue("@VenueName", wifiSignal.VenueName); //string TEXT
                            insertCommand.Parameters.AddWithValue("@ScanTime", wifiSignal.ScanTime.ToString("yyyy-MM-dd HH:mm")); //string TEXT
                            insertCommand.Parameters.AddWithValue("@Accuracy", gpsSignal.Accuracy); //double REAL
                            insertCommand.Parameters.AddWithValue("@Altitude", gpsSignal.Altitude); //double REAL
                            insertCommand.Parameters.AddWithValue("@LocationStatus", gpsSignal.GetLocationStatus()); //string TEXT
                            insertCommand.Parameters.AddWithValue("@Latitude", gpsSignal.Latitude); //double REAL
                            insertCommand.Parameters.AddWithValue("@Longitude", gpsSignal.Longitude); //double REAL
                            insertCommand.Parameters.AddWithValue("@TimeStamp", gpsSignal.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss.fff")); //string TEXT

                            try
                            {
                                insertCommand.ExecuteNonQuery();
                            }
                            catch (SqliteException e)
                            {
                                throw new Exception("SQL table INSERT not performed" + count.ToString());
                            }
                        }
                    }
                    database.Close(); database.Dispose();
                }

                Output.ItemsSource = ReadWifiScannerDatabase;
            }
        }

        private List<String> ReadWifiScannerDatabase
        {
            get
            {
                List<String> entries = new List<string>();

                using (SqliteConnection database = new SqliteConnection("Filename = WiFiScanner.db"))
                {
                    database.Open();

                    SqliteCommand sqlSelectCommand = new SqliteCommand(
                        "SELECT Ssid, Bssid, NetworkRssiInDecibelMilliwatts, TimeStamp, VenueName, Uptime, Accuracy, Altitude, LocationStatus, Latitude, Longitude " +
                        "FROM " + RemoveWhiteSpace(VenueName) + " " +
                        "ORDER BY NetworkRssiInDecibelMilliwatts DESC, Uptime DESC", database);
                    SqliteDataReader query;

                    try
                    {
                        query = sqlSelectCommand.ExecuteReader();
                    }
                    catch (SqliteException e)
                    {
                        throw new Exception("SQL database no entries in table." + RemoveWhiteSpace(VenueName));
                        //return entries;
                    }

                    while (query.Read())
                    {
                        TimeSpan interval = TimeSpan.FromTicks(query.GetInt64(5));
                        string uptime = interval.ToString("%d") + " day(s) " + interval.ToString(@"hh\:mm");

                        entries.Add("[" + query.GetString(4) + "] "
                            + query.GetString(0) + " [MAC " + query.GetString(1) + "] "
                            + query.GetString(2) + " dBm "
                            + query.GetString(3) + " "
                            + "Uptime:" + uptime + " "
                            + "Accuracy:" + query.GetFloat(6) + "m  "
                            + "Altitude:" + query.GetFloat(7) + "m "
                            + "LocationStatus:" + query.GetString(8) + " "
                            + "Latitude:" + query.GetFloat(9) + " "
                            + "Longitude:" + query.GetFloat(10));
                    }

                    database.Close(); database.Dispose();
                }
                return entries;
            }
        }
    }
}