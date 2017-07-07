using System;
using System.Collections.Generic;

namespace WiFiScannerUWP

{
    public class WiFiGPSDetail
    {
        public DateTimeOffset TimeStamp { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public double? Accuracy { get; set; }

        public List<WiFiSignal> WiFiSignals { get; private set; }

        public WiFiGPSDetail()
        {
            WiFiSignals = new List<WiFiSignal>();
        }
    }
}