using Cstl.IndoorPositioning.Abstractions.Models;

namespace Cstl.IndoorPositioning.Abstractions.Interfaces
{
    /// <summary>
    /// Converts beacon readings and known beacon anchors into a position estimate.
    /// </summary>
    public interface IBeaconLocator
    {
        /// <summary>
        /// Locates a device from BLE readings and configured anchors.
        /// </summary>
        TrilaterationResult Locate(IEnumerable<BeaconReading> readings, IEnumerable<BeaconAnchor> anchors);

        /// <summary>
        /// Resets any stateful smoothing component used by the implementation.
        /// </summary>
        void ResetSmoothing();
    }
}
