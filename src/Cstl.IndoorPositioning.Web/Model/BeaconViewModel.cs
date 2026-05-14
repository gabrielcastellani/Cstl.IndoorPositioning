namespace Cstl.IndoorPositioning.Web.Model
{
    public class BeaconViewModel
    {
        public string MAC { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int TxPower { get; set; } = -59;
        public int RSSI { get; set; } = -70;
        public bool Active { get; set; } = true;
        public double EstimatedDistanceMeters => BeaconDistanceCalculator.Calculate(RSSI, TxPower);
    }
}
