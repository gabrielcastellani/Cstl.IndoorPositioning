namespace Cstl.IndoorPositioning.Abstractions.Models
{
    public class BeaconReading
    {
        public string MAC { get; set; } = string.Empty;
        public int RSSI { get; set; }
        public int TxPower { get; set; }
    }
}
