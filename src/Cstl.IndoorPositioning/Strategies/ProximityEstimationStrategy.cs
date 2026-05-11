using Cstl.IndoorPositioning.Abstractions.Enums;
using Cstl.IndoorPositioning.Abstractions.Models;
using System.Collections.Generic;

namespace Cstl.IndoorPositioning.Strategies
{
    internal sealed class ProximityEstimationStrategy : IPositionEstimationStrategy
    {
        public bool CanEstimate(int beaconCount)
        {
            return beaconCount == 1;
        }

        public TrilaterationResult Estimate(IReadOnlyList<BeaconSample> beacons)
        {
            var beacon = beacons[0];

            return new TrilaterationResult
            {
                Latitude = beacon.Latitude,
                Longitude = beacon.Longitude,
                BeaconsUsed = 1,
                AccuracyMeters = beacon.EstimatedDistanceMeters,
                Method = EstimationMethod.Proximity
            };
        }
    }
}
