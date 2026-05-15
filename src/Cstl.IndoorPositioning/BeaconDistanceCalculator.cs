using Cstl.IndoorPositioning.Abstractions.Enums;
using Cstl.IndoorPositioning.Abstractions.Extensions;
using Cstl.IndoorPositioning.Algorithms.Distance;

namespace Cstl.IndoorPositioning
{
    /// <summary>
    /// Static convenience facade for BLE RSSI distance calculations.
    /// </summary>
    public static class BeaconDistanceCalculator
    {
        /// <summary>Default calibrated TxPower at one meter, in dBm.</summary>
        public const int DefaultTxPower = LogDistanceBeaconDistanceCalculator.DefaultTxPower;

        /// <summary>Default path-loss exponent for indoor environments.</summary>
        public const double DefaultPathLossExponent = LogDistanceBeaconDistanceCalculator.DefaultPathLossExponent;

        /// <summary>
        /// Estimates distance in meters from RSSI.
        /// </summary>
        public static double Calculate(int rssi, int? txPower = null, double pathLossExponent = DefaultPathLossExponent)
        {
            return new LogDistanceBeaconDistanceCalculator(pathLossExponent).CalculateDistance(rssi, txPower);
        }

        /// <summary>
        /// Estimates distance in meters from RSSI using an environment profile.
        /// </summary>
        public static double Calculate(int rssi, int? txPower, EnvironmentProfile profile)
        {
            return LogDistanceBeaconDistanceCalculator.ForProfile(profile).CalculateDistance(rssi, txPower);
        }

        /// <summary>
        /// Returns expected RSSI for a known distance.
        /// </summary>
        public static double DistanceToRssi(double distanceMeters, int? txPower = null, double pathLossExponent = DefaultPathLossExponent)
        {
            return new LogDistanceBeaconDistanceCalculator(pathLossExponent).CalculateExpectedRssi(distanceMeters, txPower);
        }

        /// <summary>
        /// Returns the suggested path-loss exponent for an environment profile.
        /// </summary>
        public static double PathLossExponentFor(EnvironmentProfile profile)
        {
            return profile.ToPathLossExponent();
        }
    }
}
