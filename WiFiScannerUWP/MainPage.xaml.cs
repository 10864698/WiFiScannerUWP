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
using Microsoft.Identity.Client;
using System.Linq;

namespace WiFiScannerUWP
{
    public sealed partial class MainPage : Page
    {
        ////Set the API Endpoint to Graph 'me' endpoint
        //string _graphAPIEndpoint = "https://graph.microsoft.com/v1.0/me";

        ////Set the scope for API call to user.read
        //string[] _scopes = new string[] { "user.read" };


        private WifiAdapterScanner _wifiScanner;


        public MainPage()
        {
            InitializeComponent();

            SqliteEngine.UseWinSqlite3(); //Configuring library to use SDK version of SQLite

            _wifiScanner = new WifiAdapterScanner();

            DataContext = _wifiScanner;

            CreateWifiScannerDatabase();

            Output.ItemsSource = ReadWifiScannerDatabase;
        }

        private async void PageLoaded(object sender, RoutedEventArgs e)
        {
            await InitializeScanner();
        }

        private async Task InitializeScanner()
        {
            await _wifiScanner.InitializeScanner();
        }

        private async void ClearButtonClick(object sender, RoutedEventArgs e)
        {
            ButtonClear.IsEnabled = false;

            try
            {
                ClearWifiSignalsTable();
            }
            catch (Exception ex)
            {
                MessageDialog md = new MessageDialog(ex.Message);

                await md.ShowAsync();
            }

            ButtonClear.IsEnabled = true;
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

        public async Task RunWifiScan()
        {
            await _wifiScanner.ScanForNetworks();
            WiFiNetworkReport report = _wifiScanner.WiFiAdapter.NetworkReport;

            Geolocator geolocator = new Geolocator();
            geolocator.DesiredAccuracy = PositionAccuracy.High;

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
                    VenueName = _wifiScanner.venueName,
                    ScanTime = _wifiScanner.scanTime
                };

                locationWifiGpsData.WiFiSignals.Add(wifiSignal);
                AddToWifiScannerDatabase(wifiSignal, locationWifiGpsData);

            }
        }

        private async Task ShowMessage(string message)
        {
            var dialog = new MessageDialog(message);

            await dialog.ShowAsync();
        }

        private void venueNameTextChanged(object sender, TextChangedEventArgs e)
        { }

        private void CreateWifiScannerDatabase()
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

                //String drop_table_command = "DROP TABLE IF EXISTS WiFiSignals;";
                //SqliteCommand dropTable = new SqliteCommand(drop_table_command, db);
                //try
                //{
                //    dropTable.ExecuteNonQuery();
                //}
                //catch (SqliteException e)
                //{
                //    throw new Exception("SQL table not dropped.");
                //}

                String sqlCreateTableCommand = "CREATE TABLE IF NOT EXISTS WiFiSignals (" +
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
                database.Close();
            }

            Output.ItemsSource = ReadWifiScannerDatabase;

        }

        private void ClearWifiSignalsTable()
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

                String sqlDropTableCommand = "DROP TABLE IF EXISTS WiFiSignals;";
                SqliteCommand dropTable = new SqliteCommand(sqlDropTableCommand, database);
                try
                {
                    dropTable.ExecuteNonQuery();
                }
                catch (SqliteException e)
                {
                    throw new Exception("SQL table not dropped.");
                }

                String sqlCreateTableCommand = "CREATE TABLE IF NOT EXISTS WiFiSignals (" +
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
                database.Close();
            }

            Output.ItemsSource = ReadWifiScannerDatabase;

        }

        private void AddToWifiScannerDatabase(WifiSignal wifiSignal, WifiGpsDetail gpsSignal)
        {
            using (SqliteConnection database = new SqliteConnection("Filename = WiFiScanner.db"))
            {
                database.Open();

                //check if VenueName / Bssid / Ssid exists already
                using (SqliteCommand sqlCheckExistingWifiSignalCommand = new SqliteCommand())
                {
                    sqlCheckExistingWifiSignalCommand.Connection = database;
                    sqlCheckExistingWifiSignalCommand.CommandText = "SELECT count(*) FROM WiFiSignals " +
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
                    database.Close();
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
                        "FROM WiFiSignals " +
                        "ORDER BY NetworkRssiInDecibelMilliwatts DESC, Uptime DESC", database);
                    SqliteDataReader query;

                    try
                    {
                        query = sqlSelectCommand.ExecuteReader();
                    }
                    catch (SqliteException e)
                    {
                        return entries;
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

                    database.Close();
                }
                return entries;
            }
        }

        ///// <summary>
        ///// Call AcquireTokenAsync - to acquire a token requiring user to sign-in
        ///// </summary>
        //private async void CallGraphButton_Click(object sender, RoutedEventArgs e)
        //{
        //    AuthenticationResult authResult = null;

        //    try
        //    {
        //        if (authResult == null)
        //        {
        //            authResult = await App.PublicClientApp.AcquireTokenSilentAsync(_scopes, App.PublicClientApp.Users.FirstOrDefault());
        //        }
        //    }
        //    catch (MsalUiRequiredException ex)
        //    {
        //        // A MsalUiRequiredException happened on AcquireTokenSilentAsync. This indicates you need to call AcquireTokenAsync to acquire a token
        //        System.Diagnostics.Debug.WriteLine($"MsalUiRequiredException: {ex.Message}");

        //        try
        //        {
        //            authResult = await App.PublicClientApp.AcquireTokenAsync(_scopes);
        //        }
        //        catch (MsalException msalex)
        //        {
        //            ResultText.Text = $"Error Acquiring Token:{System.Environment.NewLine}{msalex}";
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ResultText.Text = $"Error Acquiring Token Silently:{System.Environment.NewLine}{ex}";
        //        return;
        //    }

        //    if (authResult != null)
        //    {
        //        ResultText.Text = await GetHttpContentWithToken(_graphAPIEndpoint, authResult.AccessToken);
        //        DisplayBasicTokenInfo(authResult);
        //        this.SignOutButton.Visibility = Visibility.Visible;
        //    }
        //}

        ///// <summary>
        ///// Perform an HTTP GET request to a URL using an HTTP Authorization header
        ///// </summary>
        ///// <param name="url">The URL</param>
        ///// <param name="token">The token</param>
        ///// <returns>String containing the results of the GET operation</returns>
        //public async Task<string> GetHttpContentWithToken(string url, string token)
        //{
        //    var httpClient = new System.Net.Http.HttpClient();
        //    System.Net.Http.HttpResponseMessage response;
        //    try
        //    {
        //        var request = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, url);
        //        //Add the token in Authorization header
        //        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        //        response = await httpClient.SendAsync(request);
        //        var content = await response.Content.ReadAsStringAsync();
        //        return content;
        //    }
        //    catch (Exception ex)
        //    {
        //        return ex.ToString();
        //    }
        //}

        ///// <summary>
        ///// Sign out the current user
        ///// </summary>
        //private void SignOutButton_Click(object sender, RoutedEventArgs e)
        //{
        //    if (App.PublicClientApp.Users.Any())
        //    {
        //        try
        //        {
        //            App.PublicClientApp.Remove(App.PublicClientApp.Users.FirstOrDefault());
        //            this.ResultText.Text = "User has signed-out";
        //            this.CallGraphButton.Visibility = Visibility.Visible;
        //            this.SignOutButton.Visibility = Visibility.Collapsed;
        //        }
        //        catch (MsalException ex)
        //        {
        //            ResultText.Text = $"Error signing-out user: {ex.Message}";
        //        }
        //    }
        //}

        ///// <summary>
        ///// Display basic information contained in the token
        ///// </summary>
        //private void DisplayBasicTokenInfo(AuthenticationResult authResult)
        //{
        //    TokenInfoText.Text = "";
        //    if (authResult != null)
        //    {
        //        TokenInfoText.Text += $"Name: {authResult.User.Name}" + Environment.NewLine;
        //        TokenInfoText.Text += $"Username: {authResult.User.DisplayableId}" + Environment.NewLine;
        //        TokenInfoText.Text += $"Token Expires: {authResult.ExpiresOn.ToLocalTime()}" + Environment.NewLine;
        //        TokenInfoText.Text += $"Access Token: {authResult.AccessToken}" + Environment.NewLine;
        //    }
        //}

    }
}