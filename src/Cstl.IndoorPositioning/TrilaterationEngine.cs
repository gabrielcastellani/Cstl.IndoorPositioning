using Cstl.IndoorPositioning.Abstractions.Models;
using Cstl.IndoorPositioning.Algorithms;
using Cstl.IndoorPositioning.Internal;

namespace Cstl.IndoorPositioning
{
    /// <summary>
    /// Static convenience facade for position estimation from prepared beacon samples.
    /// </summary>
    public static class TrilaterationEngine
    {
        private static readonly WeightedLeastSquaresPositionEstimator Estimator = new WeightedLeastSquaresPositionEstimator();

        /// <summary>
        /// Estimates a position from prepared beacon samples.
        /// </summary>
        public static TrilaterationResult Estimate(IEnumerable<BeaconSample> beacons)
        {
            return Estimator.Estimate(beacons.ToReadOnlyList());
        }

        /// <summary>
        /// Estimates a position from prepared beacon samples.
        /// </summary>
        public static TrilaterationResult Estimate(IReadOnlyList<BeaconSample> beacons)
        {
            return Estimator.Estimate(beacons);
        }
    }
}
