namespace Cstl.IndoorPositioning.Algorithms.Squares
{
    internal readonly struct LocalPoint
    {
        public double X { get; }
        public double Y { get; }
        public double DistanceMeters { get; }
        public double Weight { get; }

        public LocalPoint(double x, double y, double distanceMeters, double weight)
        {
            X = x;
            Y = y;
            DistanceMeters = distanceMeters;
            Weight = weight;
        }
    }
}
