using System;

namespace Cstl.IndoorPositioning.Weighting
{
    internal sealed class InverseSquareDistanceWeightCalculator : IBeaconWeightCalculator
    {
        private const double MinimumDistanceMeters = 0.1;

        public double Calculate(double distanceMeters)
        {
            var safeDistance = Math.Max(distanceMeters, MinimumDistanceMeters);

            return 1.0 / (safeDistance * safeDistance);
        }
    }
}
