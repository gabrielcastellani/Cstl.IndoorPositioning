namespace Cstl.IndoorPositioning.Abstractions.Models
{
    public sealed class BeaconSample
    {
        public string MAC { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double EstimatedDistanceMeters { get; set; }
        public int RSSI { get; set; }
        public int TxPower { get; set; }

        public static BeaconSample From(BeaconReading reading, BeaconAnchor anchor)
        {
            return new BeaconSample()
            {
                MAC = reading.MAC,
                Latitude = anchor.Latitude,
                Longitude = anchor.Longitude,
                RSSI = reading.RSSI,
                TxPower = reading.TxPower
            };
        }
    }
}
