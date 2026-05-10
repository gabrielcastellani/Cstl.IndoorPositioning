namespace Cstl.IndoorPositioning.Abstractions.Models
{
    public class BeaconAnchor
    {
        public string MAC { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
