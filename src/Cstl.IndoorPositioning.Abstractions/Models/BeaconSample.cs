namespace Cstl.IndoorPositioning.Abstractions.Models
{
    public class BeaconSample
    {
        public string MAC { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int RSSI { get; set; }
        public int TxPower { get; set; }
        public double EstimatedDistanceMeters { get; set; }

        public static BeaconSample From(BeaconAnchor anchor, BeaconReading reading)
        {
            return new BeaconSample()
            {
                MAC = anchor.MAC,
                Latitude = anchor.Latitude,
                Longitude = anchor.Longitude,
                RSSI = reading.RSSI,
                TxPower = reading.TxPower,
            };
        }
    }
}
