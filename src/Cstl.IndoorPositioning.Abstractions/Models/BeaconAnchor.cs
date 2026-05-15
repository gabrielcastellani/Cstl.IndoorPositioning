namespace Cstl.IndoorPositioning.Abstractions.Models
{
    /// <summary>
    /// Beacon with a known geographic position.
    /// </summary>
    public sealed class BeaconAnchor
    {
        /// <summary>Beacon MAC address.</summary>
        public MacAddress Mac { get; }

        /// <summary>Known beacon position.</summary>
        public GeoPosition Position { get; }

        /// <summary>Latitude in decimal degrees.</summary>
        public double Latitude => Position.Latitude;

        /// <summary>Longitude in decimal degrees.</summary>
        public double Longitude => Position.Longitude;

        /// <summary>
        /// Creates a beacon anchor.
        /// </summary>
        public BeaconAnchor(MacAddress mac, GeoPosition position)
        {
            if (mac.IsEmpty)
                throw new ArgumentException("MAC address cannot be empty.", nameof(mac));

            Mac = mac;
            Position = position;
        }

        /// <summary>
        /// Convenience constructor from primitive values.
        /// </summary>
        public BeaconAnchor(string mac, double latitude, double longitude)
            : this(new MacAddress(mac), new GeoPosition(latitude, longitude))
        { }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Mac} @ {Position}";
        }
    }
}
