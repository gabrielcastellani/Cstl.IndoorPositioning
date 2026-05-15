using Cstl.IndoorPositioning.Abstractions.Interfaces;
using Cstl.IndoorPositioning.Abstractions.Internal;

namespace Cstl.IndoorPositioning.Abstractions.Models
{
    /// <summary>
    /// Beacon reading enriched with a known anchor position and estimated distance.
    /// </summary>
    public sealed class BeaconSample
    {
        /// <summary>Beacon MAC address.</summary>
        public MacAddress Mac { get; }

        /// <summary>Known beacon position.</summary>
        public GeoPosition Position { get; }

        /// <summary>Latitude in decimal degrees.</summary>
        public double Latitude => Position.Latitude;

        /// <summary>Longitude in decimal degrees.</summary>
        public double Longitude => Position.Longitude;

        /// <summary>Received signal strength in dBm.</summary>
        public int Rssi { get; }

        /// <summary>Calibrated TxPower at one meter in dBm. Null when unknown.</summary>
        public int? TxPower { get; }

        /// <summary>Estimated distance in meters.</summary>
        public double EstimatedDistanceMeters { get; }

        /// <summary>
        /// Creates a beacon sample.
        /// </summary>
        public BeaconSample(
            MacAddress mac,
            GeoPosition position,
            int rssi,
            int? txPower,
            double estimatedDistanceMeters)
        {
            if (mac.IsEmpty)
                throw new ArgumentException("MAC address cannot be empty.", nameof(mac));

            SignalValidation.ThrowIfInvalidRssi(rssi, nameof(rssi));

            if (txPower.HasValue)
            {
                SignalValidation.ThrowIfInvalidTxPower(txPower.Value, nameof(txPower));
            }

            DistanceValidation.ThrowIfNotPositiveOrInvalid(estimatedDistanceMeters, nameof(estimatedDistanceMeters));

            Mac = mac;
            Position = position;
            Rssi = rssi;
            TxPower = txPower;
            EstimatedDistanceMeters = estimatedDistanceMeters;
        }

        /// <summary>
        /// Convenience constructor from primitive values. A TxPower value of zero is treated as unknown.
        /// </summary>
        public BeaconSample(
            string mac,
            double latitude,
            double longitude,
            int rssi,
            int txPower,
            double estimatedDistanceMeters)
            : this(new MacAddress(mac), new GeoPosition(latitude, longitude), rssi, txPower == 0 ? (int?)null : txPower, estimatedDistanceMeters)
        { }

        /// <summary>
        /// Creates a sample from a reading, an anchor and a distance calculator.
        /// </summary>
        public static BeaconSample From(
            BeaconReading reading,
            BeaconAnchor anchor,
            IBeaconDistanceCalculator distanceCalculator)
        {
            if (reading is null)
                throw new ArgumentNullException(nameof(reading));
            if (anchor is null)
                throw new ArgumentNullException(nameof(anchor));
            if (distanceCalculator is null)
                throw new ArgumentNullException(nameof(distanceCalculator));

            return new BeaconSample(
                reading.Mac,
                anchor.Position,
                reading.Rssi,
                reading.TxPower,
                distanceCalculator.CalculateDistance(reading));
        }
    }
}
