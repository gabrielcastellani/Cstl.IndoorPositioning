using System;

namespace Cstl.IndoorPositioning
{
    public static class BeaconDistanceCalculator
    {
        public const double DefaultPathLossExponent = 3.0;
        private const double MinimumDistanceMeters = 0.01;

        public static double Calculate(int rssi, int txPower, double pathLossExponent = DefaultPathLossExponent)
        {
            GuardPathLossExponent(pathLossExponent);

            var ratio = (txPower - rssi) / (10.0 * pathLossExponent);
            return Math.Pow(10.0, ratio);
        }

        public static double DistanceToRssi(double distanceMeters, int txPower, double pathLossExponent = DefaultPathLossExponent)
        {
            GuardDistanceMeters(distanceMeters);
            GuardPathLossExponent(pathLossExponent);

            return txPower - (10.0 * pathLossExponent * Math.Log10(distanceMeters));
        }

        private static void GuardPathLossExponent(double pathLossExponent)
        {
            if (pathLossExponent <= 0)
                throw new ArgumentOutOfRangeException(nameof(pathLossExponent), "Path loss exponent must be positive.");
        }

        private static void GuardDistanceMeters(double distanceMeters)
        {
            if (distanceMeters < MinimumDistanceMeters)
                throw new ArgumentOutOfRangeException(nameof(distanceMeters), $"Distance must be at least {MinimumDistanceMeters}m.");
        }
    }
}
