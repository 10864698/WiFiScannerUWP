using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.WiFi;



namespace WiFiScannerUWP
{
    public class WiFiScanner : INotifyPropertyChanged

    {
        private string venue_name;

        public WiFiAdapter WiFiAdapter { get; private set; }

        public string venueName

        {
            get { return venue_name; }

            set
            {
                venue_name = value;
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
                var startTime = DateTime.Now;

                await WiFiAdapter.ScanAsync();

                var endTime = DateTime.Now;

                var duration = endTime - startTime;

                var time = duration.ToString();
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