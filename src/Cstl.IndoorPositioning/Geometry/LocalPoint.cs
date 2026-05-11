namespace Cstl.IndoorPositioning.Geometry
{
    internal sealed class LocalPoint
    {
        public double X { get; }
        public double Y { get; }
        public double Distance { get; }
        public double Weight { get; }

        public LocalPoint(double x, double y, double distance, double weight)
        {
            X = x;
            Y = y;
            Distance = distance;
            Weight = weight;
        }
    }
}
