using System.Globalization;

namespace Cstl.IndoorPositioning.Abstractions.Models
{
    /// <summary>
    /// Immutable geographic position in WGS-84 decimal degrees.
    /// </summary>
    public readonly struct GeoPosition : IEquatable<GeoPosition>
    {
        /// <summary>Latitude in decimal degrees.</summary>
        public double Latitude { get; }

        /// <summary>Longitude in decimal degrees.</summary>
        public double Longitude { get; }

        /// <summary>
        /// Creates a geographic position.
        /// </summary>
        public GeoPosition(double latitude, double longitude)
        {
            if (latitude < -90.0 || latitude > 90.0 || double.IsNaN(latitude) || double.IsInfinity(latitude))
                throw new ArgumentOutOfRangeException(nameof(latitude), "Latitude must be between -90 and 90 degrees.");

            if (longitude < -180.0 || longitude > 180.0 || double.IsNaN(longitude) || double.IsInfinity(longitude))
                throw new ArgumentOutOfRangeException(nameof(longitude), "Longitude must be between -180 and 180 degrees.");

            Latitude = latitude;
            Longitude = longitude;
        }

        /// <summary>Deconstructs the position.</summary>
        public void Deconstruct(out double latitude, out double longitude)
        {
            latitude = Latitude;
            longitude = Longitude;
        }

        /// <inheritdoc />
        public bool Equals(GeoPosition other)
        {
            return Latitude.Equals(other.Latitude) && Longitude.Equals(other.Longitude);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is GeoPosition other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return (Latitude.GetHashCode() * 397) ^ Longitude.GetHashCode();
            }
        }

        /// <summary>Compares two positions.</summary>
        public static bool operator ==(GeoPosition left, GeoPosition right) => left.Equals(right);

        /// <summary>Compares two positions.</summary>
        public static bool operator !=(GeoPosition left, GeoPosition right) => !left.Equals(right);

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "({0:F6}, {1:F6})", Latitude, Longitude);
        }
    }
}
