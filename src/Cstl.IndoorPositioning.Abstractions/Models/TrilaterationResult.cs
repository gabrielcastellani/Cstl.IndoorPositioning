using Cstl.IndoorPositioning.Abstractions.Enums;
using Cstl.IndoorPositioning.Abstractions.Internal;
using System.Globalization;

namespace Cstl.IndoorPositioning.Abstractions.Models
{
    /// <summary>
    /// Position estimation result.
    /// </summary>
    public sealed class TrilaterationResult
    {
        /// <summary>Estimated position.</summary>
        public GeoPosition Position { get; }

        /// <summary>Latitude in decimal degrees.</summary>
        public double Latitude => Position.Latitude;

        /// <summary>Longitude in decimal degrees.</summary>
        public double Longitude => Position.Longitude;

        /// <summary>Number of beacons used by the estimate.</summary>
        public int BeaconsUsed { get; }

        /// <summary>Estimated accuracy or residual error in meters.</summary>
        public double AccuracyMeters { get; }

        /// <summary>Algorithm used to produce the estimate.</summary>
        public EstimationMethod Method { get; }

        /// <summary>
        /// Creates a position estimation result.
        /// </summary>
        public TrilaterationResult(
            GeoPosition position,
            int beaconsUsed,
            double accuracyMeters,
            EstimationMethod method)
        {
            if (beaconsUsed <= 0)
                throw new ArgumentOutOfRangeException(nameof(beaconsUsed), "At least one beacon must be used.");

            DistanceValidation.ThrowIfNegativeOrInvalid(accuracyMeters, nameof(accuracyMeters));

            Position = position;
            BeaconsUsed = beaconsUsed;
            AccuracyMeters = accuracyMeters;
            Method = method;
        }

        /// <summary>
        /// Convenience constructor from latitude and longitude.
        /// </summary>
        public TrilaterationResult(
            double latitude,
            double longitude,
            int beaconsUsed,
            double accuracyMeters,
            EstimationMethod method)
            : this(new GeoPosition(latitude, longitude), beaconsUsed, accuracyMeters, method)
        { }

        /// <inheritdoc />
        public override string ToString()
            => string.Format(
                CultureInfo.InvariantCulture,
                "{0} ± {1:F2} m ({2}, {3} beacon(s))",
                Position,
                AccuracyMeters,
                Method,
                BeaconsUsed);
    }
}
