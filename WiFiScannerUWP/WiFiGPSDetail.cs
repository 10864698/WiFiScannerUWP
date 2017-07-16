using System;
using System.Collections.Generic;
using Windows.Devices.Geolocation;

namespace WiFiScannerUWP

{
    public class WifiGpsDetail
    {
        public double? Accuracy { get; set; }
        public double? Altitude { get; set; }
        public PositionStatus LocationStatus { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public DateTimeOffset TimeStamp { get; set; }

        public List<WifiSignal> WiFiSignals { get; private set; }

        public WifiGpsDetail()
        {
            WiFiSignals = new List<WifiSignal>();
        }

        public string GetLocationStatus()
        {
            if (LocationStatus == PositionStatus.Disabled)
                return "Disabled";
            if (LocationStatus == PositionStatus.Initializing)
                return "Initializing";
            if (LocationStatus == PositionStatus.NoData)
                return "NoData";
            if (LocationStatus == PositionStatus.NotAvailable)
                return "NotAvailable";
            if (LocationStatus == PositionStatus.NotInitialized)
                return "NotInitialized";
            if (LocationStatus == PositionStatus.Ready)
                return "Ready";
            else return "NotAvailable";
        }
    }
}