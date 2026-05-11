using Cstl.IndoorPositioning.Geometry;

namespace Cstl.IndoorPositioning.Accuracy
{
    internal interface IAccuracyCalculator
    {
        double Calculate(LocalPoint[] points, LocalCoordinate estimatedCoordinate);
    }
}
