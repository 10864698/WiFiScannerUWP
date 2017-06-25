using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WiFiScannerUWP

{
    public class WiFiPointData
    {
        public DateTimeOffset TimeStamp { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public double? Accuracy { get; set; }

        public List<WiFiSignal> WiFiSignals { get; private set; }

        public WiFiPointData()
        {
            WiFiSignals = new List<WiFiSignal>();
        }
    }
}