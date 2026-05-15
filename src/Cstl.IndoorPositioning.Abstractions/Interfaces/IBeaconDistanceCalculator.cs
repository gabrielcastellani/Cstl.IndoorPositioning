using Cstl.IndoorPositioning.Abstractions.Models;

namespace Cstl.IndoorPositioning.Abstractions.Interfaces
{
    /// <summary>
    /// Calculates an estimated distance from BLE signal data.
    /// </summary>
    public interface IBeaconDistanceCalculator
    {
        /// <summary>
        /// Calculates the estimated distance in meters for a beacon reading.
        /// </summary>
        double CalculateDistance(BeaconReading reading);

        /// <summary>
        /// Calculates the estimated distance in meters from RSSI and optional calibrated TxPower.
        /// </summary>
        double CalculateDistance(int rssi, int? txPower = null);

        /// <summary>
        /// Calculates the expected RSSI for a known distance.
        /// </summary>
        double CalculateExpectedRssi(double distanceMeters, int? txPower = null);
    }
}
