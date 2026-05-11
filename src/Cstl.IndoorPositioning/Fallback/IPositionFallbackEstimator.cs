using Cstl.IndoorPositioning.Abstractions.Models;
using System.Collections.Generic;

namespace Cstl.IndoorPositioning.Fallback
{
    internal interface IPositionFallbackEstimator
    {
        TrilaterationResult Estimate(IReadOnlyList<BeaconSample> beacons);
    }
}
