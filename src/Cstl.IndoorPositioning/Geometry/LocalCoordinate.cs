namespace Cstl.IndoorPositioning.Geometry
{
    internal sealed class LocalCoordinate
    {
        public double X { get; }
        public double Y { get; }

        public LocalCoordinate(double x, double y)
        {
            X = x;
            Y = y;
        }
    }
}
