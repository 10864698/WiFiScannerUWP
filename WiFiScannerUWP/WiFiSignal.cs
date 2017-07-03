using System;

namespace WiFiScannerUWP

{
    public class WiFiSignal
    {
        public TimeSpan BeaconInterval { get; set; }
        public string Bssid { get; set; }
        public double ChannelCenterFrequencyInKilohertz { get; set; }
        public bool IsWiFiDirect { get; internal set; }
        public string NetworkKind { get; set; }
        public double NetworkRssiInDecibelMilliwatts { get; internal set; }
        public string PhyKind { get; set; }
        public string SecuritySettings { get; internal set; }
        public byte SignalBars { get; set; }
        public string Ssid { get; set; }
        public TimeSpan Uptime { get; internal set; }
        public string VenueName { get; internal set; }
        public DateTime ScanTime { get; internal set; }

        public WiFiSignal()
        {
            VenueName = "VenueName";
        }

    }
}