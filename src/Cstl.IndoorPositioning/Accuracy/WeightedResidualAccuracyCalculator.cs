using Cstl.IndoorPositioning.Geometry;
using System;

namespace Cstl.IndoorPositioning.Accuracy
{
    internal sealed class WeightedResidualAccuracyCalculator : IAccuracyCalculator
    {
        public double Calculate(LocalPoint[] points, LocalCoordinate estimatedCoordinate)
        {
            var totalWeight = 0.0;
            var weightedError = 0.0;

            foreach (var point in points)
            {
                var actualDistance = EuclideanDistance(
                    x1: estimatedCoordinate.X,
                    y1: estimatedCoordinate.Y,
                    x2: point.X,
                    y2: point.Y);

                var residualError = Math.Abs(actualDistance - point.Distance);

                weightedError += point.Weight * residualError;
                totalWeight += point.Weight;
            }

            return totalWeight > 0
                ? weightedError / totalWeight
                : double.NaN;
        }

        private static double EuclideanDistance(double x1, double y1, double x2, double y2)
        {
            var distanceX = x1 - x2;
            var distanceY = y1 - y2;

            return Math.Sqrt((distanceX * distanceX) + (distanceY * distanceY));
        }
    }
}
