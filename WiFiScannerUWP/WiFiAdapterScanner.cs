using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.WiFi;



namespace WiFiScannerUWP
{
    
    public class WifiAdapterScanner : INotifyPropertyChanged

    {
        Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

        private string _venueName = "Enter Venue Name";
        internal DateTime scanTime;

        public WiFiAdapter WiFiAdapter { get; private set; }

        public string venueName
        {
            get
            {
                return _venueName;
            }
            set
            {
                _venueName = value;
                OnPropertyChanged("venueName");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public async Task InitializeScanner()
        {
            await InitializeFirstAdapter();
        }

        public async Task ScanForNetworks()
        {
            if (WiFiAdapter != null)
            {
                scanTime = DateTime.Now;

                await WiFiAdapter.ScanAsync();
            }
        }

        private async Task InitializeFirstAdapter()
        {
            var access = await WiFiAdapter.RequestAccessAsync();

            if (access != WiFiAccessStatus.Allowed)
            {
                throw new Exception("WiFiAccessStatus not allowed");
            }
            else
            {
                var wifiAdapterResults = await DeviceInformation.FindAllAsync(WiFiAdapter.GetDeviceSelector());

                if (wifiAdapterResults.Count >= 1)
                {
                    WiFiAdapter = await WiFiAdapter.FromIdAsync(wifiAdapterResults[0].Id);
                }
                else
                {
                    throw new Exception("WiFi Adapter not found.");
                }
            }
        }
    }
}