using Cstl.IndoorPositioning.Abstractions.Internal;

namespace Cstl.IndoorPositioning.Abstractions.Models
{
    /// <summary>
    /// Single BLE beacon reading received by a device.
    /// </summary>
    public sealed class BeaconReading
    {
        /// <summary>Beacon MAC address.</summary>
        public MacAddress Mac { get; }

        /// <summary>Received signal strength in dBm.</summary>
        public int Rssi { get; }

        /// <summary>Calibrated TxPower at one meter in dBm. Null when unknown.</summary>
        public int? TxPower { get; }

        /// <summary>
        /// Creates a beacon reading.
        /// </summary>
        public BeaconReading(MacAddress mac, int rssi, int? txPower = null)
        {
            if (mac.IsEmpty)
                throw new ArgumentException("MAC address cannot be empty.", nameof(mac));

            SignalValidation.ThrowIfInvalidRssi(rssi, nameof(rssi));

            if (txPower.HasValue)
            {
                SignalValidation.ThrowIfInvalidTxPower(txPower.Value, nameof(txPower));
            }

            Mac = mac;
            Rssi = rssi;
            TxPower = txPower;
        }

        /// <summary>
        /// Convenience constructor from primitive values. A TxPower value of zero is treated as unknown.
        /// </summary>
        public BeaconReading(string mac, int rssi, int txPower = 0)
            : this(new MacAddress(mac), rssi, NormalizeTxPower(txPower))
        { }

        private static int? NormalizeTxPower(int txPower)
        {
            return txPower == 0 ? (int?)null : txPower;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return TxPower.HasValue ? $"{Mac} RSSI={Rssi} TxPower={TxPower}" : $"{Mac} RSSI={Rssi}";
        }
    }
}
