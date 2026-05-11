using Cstl.IndoorPositioning.Abstractions.Enums;
using Cstl.IndoorPositioning.Abstractions.Models;
using Cstl.IndoorPositioning.Weighting;
using System.Collections.Generic;
using System.Linq;

namespace Cstl.IndoorPositioning.Fallback
{
    internal sealed class WeightedCentroidFallbackEstimator : IPositionFallbackEstimator
    {
        private readonly IBeaconWeightCalculator _weightCalculator;

        public WeightedCentroidFallbackEstimator(IBeaconWeightCalculator weightCalculator)
        {
            _weightCalculator = weightCalculator;
        }

        public TrilaterationResult Estimate(IReadOnlyList<BeaconSample> beacons)
        {
            var totalWeight = 0.0;
            var latitudeSum = 0.0;
            var longitudeSum = 0.0;

            foreach (var beacon in beacons)
            {
                var weight = _weightCalculator.Calculate(beacon.EstimatedDistanceMeters);

                latitudeSum += beacon.Latitude * weight;
                longitudeSum += beacon.Longitude * weight;
                totalWeight += weight;
            }

            return new TrilaterationResult
            {
                Latitude = latitudeSum / totalWeight,
                Longitude = longitudeSum / totalWeight,
                BeaconsUsed = beacons.Count,
                AccuracyMeters = beacons.Average(x => x.EstimatedDistanceMeters),
                Method = EstimationMethod.Trilateration
            };
        }
    }
}
