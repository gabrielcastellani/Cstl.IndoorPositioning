using Cstl.IndoorPositioning.Abstractions.Enums;

namespace Cstl.IndoorPositioning.Abstractions.Models
{
    public class TrilaterationResult
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int BeaconsUsed { get; set; }
        public double AccuracyMeters { get; set; }
        public EstimationMethod Method { get; set; }
    }
}
