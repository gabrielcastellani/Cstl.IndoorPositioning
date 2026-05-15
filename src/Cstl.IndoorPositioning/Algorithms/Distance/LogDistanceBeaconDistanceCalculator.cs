using Cstl.IndoorPositioning.Abstractions.Enums;
using Cstl.IndoorPositioning.Abstractions.Extensions;
using Cstl.IndoorPositioning.Abstractions.Interfaces;
using Cstl.IndoorPositioning.Abstractions.Models;
using Cstl.IndoorPositioning.Internal;

namespace Cstl.IndoorPositioning.Algorithms.Distance
{
    /// <summary>
    /// Calculates distance from RSSI using the Log-Distance Path Loss model.
    /// </summary>
    public sealed class LogDistanceBeaconDistanceCalculator : IBeaconDistanceCalculator
    {
        private readonly int _defaultTxPower;
        private readonly double _pathLossExponent;

        /// <summary>Default calibrated TxPower at one meter, in dBm.</summary>
        public const int DefaultTxPower = -59;

        /// <summary>Default path-loss exponent for typical indoor environments.</summary>
        public const double DefaultPathLossExponent = 3.0;

        /// <summary>Minimum returned distance in meters.</summary>
        public const double MinimumDistanceMeters = 0.01;

        /// <summary>Configured path-loss exponent.</summary>
        public double PathLossExponent => _pathLossExponent;

        /// <summary>Configured default TxPower.</summary>
        public int ConfiguredDefaultTxPower => _defaultTxPower;

        /// <summary>
        /// Creates a log-distance calculator.
        /// </summary>
        public LogDistanceBeaconDistanceCalculator(
            double pathLossExponent = DefaultPathLossExponent,
            int defaultTxPower = DefaultTxPower)
        {
            if (pathLossExponent <= 0 || double.IsNaN(pathLossExponent) || double.IsInfinity(pathLossExponent))
                throw new ArgumentOutOfRangeException(nameof(pathLossExponent), "Path-loss exponent must be positive and finite.");

            SignalValidation.ThrowIfInvalidTxPower(defaultTxPower, nameof(defaultTxPower));

            _pathLossExponent = pathLossExponent;
            _defaultTxPower = defaultTxPower;
        }

        /// <summary>
        /// Creates a calculator for an environment profile.
        /// </summary>
        public static LogDistanceBeaconDistanceCalculator ForProfile(EnvironmentProfile profile)
        {
            return new LogDistanceBeaconDistanceCalculator(profile.ToPathLossExponent());
        }

        /// <inheritdoc />
        public double CalculateDistance(BeaconReading reading)
        {
            if (reading is null)
                throw new ArgumentNullException(nameof(reading));

            return CalculateDistance(reading.Rssi, reading.TxPower);
        }

        /// <inheritdoc />
        public double CalculateDistance(int rssi, int? txPower = null)
        {
            SignalValidation.ThrowIfInvalidRssi(rssi, nameof(rssi));

            var effectiveTxPower = ResolveTxPower(txPower);
            var ratio = (effectiveTxPower - rssi) / (10.0 * _pathLossExponent);

            return Math.Max(Math.Pow(10.0, ratio), MinimumDistanceMeters);
        }

        /// <inheritdoc />
        public double CalculateExpectedRssi(double distanceMeters, int? txPower = null)
        {
            DistanceValidation.ThrowIfNotPositiveOrInvalid(distanceMeters, nameof(distanceMeters));
            if (distanceMeters < MinimumDistanceMeters)
                throw new ArgumentOutOfRangeException(nameof(distanceMeters), $"Distance must be at least {MinimumDistanceMeters} m.");

            var effectiveTxPower = ResolveTxPower(txPower);
            return effectiveTxPower - (10.0 * _pathLossExponent * Math.Log10(distanceMeters));
        }

        private int ResolveTxPower(int? txPower)
        {
            if (!txPower.HasValue)
                return _defaultTxPower;

            SignalValidation.ThrowIfInvalidTxPower(txPower.Value, nameof(txPower));
            return txPower.Value;
        }
    }
}
