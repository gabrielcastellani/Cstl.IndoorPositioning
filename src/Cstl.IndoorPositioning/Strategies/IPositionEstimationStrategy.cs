using Cstl.IndoorPositioning.Abstractions.Models;
using System.Collections.Generic;

namespace Cstl.IndoorPositioning.Strategies
{
    internal interface IPositionEstimationStrategy
    {
        bool CanEstimate(int beaconCount);
        TrilaterationResult Estimate(IReadOnlyList<BeaconSample> beacons);
    }
}
