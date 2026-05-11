namespace Cstl.IndoorPositioning.Abstractions.Models
{
    public sealed class BeaconSample
    {
        public string MAC { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double EstimatedDistanceMeters { get; set; }
    }
}
