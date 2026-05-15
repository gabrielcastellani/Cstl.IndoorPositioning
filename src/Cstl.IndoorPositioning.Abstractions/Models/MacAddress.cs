using System.Text;

namespace Cstl.IndoorPositioning.Abstractions.Models
{
    /// <summary>
    /// Immutable, normalized BLE MAC address value object.
    /// </summary>
    public readonly struct MacAddress : IEquatable<MacAddress>
    {
        private readonly string? _value;

        /// <summary>Normalized uppercase value using colon separators.</summary>
        public string Value => _value ?? string.Empty;

        /// <summary>Returns true when the value was not initialized.</summary>
        public bool IsEmpty => string.IsNullOrEmpty(Value);

        /// <summary>
        /// Creates a MAC address from a value such as AA:BB:CC:DD:EE:FF, AA-BB-CC-DD-EE-FF or AABBCCDDEEFF.
        /// </summary>
        public MacAddress(string value)
        {
            if (!TryNormalize(value, out var normalized))
                throw new ArgumentException("Invalid MAC address format. Expected a 48-bit hexadecimal address.", nameof(value));

            _value = normalized;
        }

        private MacAddress(string normalized, bool _)
        {
            _value = normalized;
        }

        /// <summary>
        /// Tries to parse and normalize a MAC address.
        /// </summary>
        public static bool TryParse(string? value, out MacAddress macAddress)
        {
            if (TryNormalize(value, out var normalized))
            {
                macAddress = new MacAddress(normalized, true);
                return true;
            }

            macAddress = default;
            return false;
        }

        /// <inheritdoc />
        public bool Equals(MacAddress other)
        {
            return StringComparer.Ordinal.Equals(Value, other.Value);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is MacAddress other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return StringComparer.Ordinal.GetHashCode(Value);
        }

        /// <inheritdoc />
        public override string ToString() => Value;

        /// <summary>Compares two addresses.</summary>
        public static bool operator ==(MacAddress left, MacAddress right) => left.Equals(right);

        /// <summary>Compares two addresses.</summary>
        public static bool operator !=(MacAddress left, MacAddress right) => !left.Equals(right);

        /// <summary>Converts the value object to its normalized string value.</summary>
        public static implicit operator string(MacAddress macAddress) => macAddress.Value;

        /// <summary>Creates a value object from a string.</summary>
        public static explicit operator MacAddress(string value) => new MacAddress(value);

        private static bool TryNormalize(string? value, out string normalized)
        {
            normalized = string.Empty;

            if (string.IsNullOrWhiteSpace(value))
                return false;

            var hex = new StringBuilder(capacity: 12);

            foreach (var character in value.Trim())
            {
                if (character == ':' || character == '-' || character == '.')
                    continue;

                if (!Uri.IsHexDigit(character))
                    return false;

                hex.Append(char.ToUpperInvariant(character));
            }

            if (hex.Length != 12)
                return false;

            var output = new char[17];
            var sourceIndex = 0;
            var targetIndex = 0;

            for (var group = 0; group < 6; group++)
            {
                if (group > 0)
                    output[targetIndex++] = ':';

                output[targetIndex++] = hex[sourceIndex++];
                output[targetIndex++] = hex[sourceIndex++];
            }

            normalized = new string(output);
            return true;
        }
    }
}
