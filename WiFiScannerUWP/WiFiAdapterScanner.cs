using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.WiFi;
using Windows.Storage;

namespace WiFiScannerUWP
{
    
    public class WifiAdapterScanner 

    {
        internal DateTime scanTime;
        public WiFiAdapter WiFiAdapter { get; private set; }

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