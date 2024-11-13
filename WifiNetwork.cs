namespace wifiSignal
{
    public class WifiNetwork
    {
        public string SSID { get; set; }
        public string SignalStrength { get; set; }

        public WifiNetwork(string ssid, string signalStrength)
        {
            SSID = ssid;
            SignalStrength = signalStrength;
        }
    }
}
