using System;

namespace WiFiScannerUWP

{
    public class WiFiSignal
    {
        public string MacAddress { get; set; }
        public string Ssid { get; set; }
        public byte SignalBars { get; set; }
        public string NetworkKind { get; set; }
        public string PhysicalKind { get; set; }
        public double ChannelCenterFrequencyInKilohertz { get; set; }
        public string Encryption { get; set; }
        public string VenueName { get; internal set; }
        public DateTime StartTime { get; internal set; }
    }
}