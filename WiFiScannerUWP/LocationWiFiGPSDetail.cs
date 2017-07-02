using System;
using System.Collections.Generic;

namespace WiFiScannerUWP

{
    public class LocationWiFiGPSDetail
    {
        public DateTimeOffset TimeStamp { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public double? Accuracy { get; set; }

        public List<WiFiSignal> WiFiSignals { get; private set; }

        public LocationWiFiGPSDetail()
        {
            WiFiSignals = new List<WiFiSignal>();
        }
    }
}