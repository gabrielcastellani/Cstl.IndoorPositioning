using Cstl.IndoorPositioning.Abstractions.Models;

namespace Cstl.IndoorPositioning.Abstractions.Interfaces
{
    /// <summary>
    /// Estimates a geographic position from prepared beacon samples.
    /// </summary>
    public interface IPositionEstimator
    {
        /// <summary>
        /// Estimates the current position. Supports one, two or three or more samples.
        /// </summary>
        TrilaterationResult Estimate(IReadOnlyList<BeaconSample> samples);
    }
}
