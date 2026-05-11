using Cstl.IndoorPositioning.Abstractions.Enums;
using Cstl.IndoorPositioning.Abstractions.Models;
using Cstl.IndoorPositioning.Accuracy;
using Cstl.IndoorPositioning.Fallback;
using Cstl.IndoorPositioning.Geometry;
using Cstl.IndoorPositioning.LinearAlgebra;
using Cstl.IndoorPositioning.Weighting;
using System.Collections.Generic;
using System.Linq;

namespace Cstl.IndoorPositioning.Strategies
{
    internal sealed class WeightedLeastSquaresEstimationStrategy : IPositionEstimationStrategy
    {
        private readonly IGeoProjection _projection;
        private readonly IBeaconWeightCalculator _weightCalculator;
        private readonly ILinearSystemSolver _solver;
        private readonly IPositionFallbackEstimator _fallbackEstimator;
        private readonly IAccuracyCalculator _accuracyCalculator;

        public WeightedLeastSquaresEstimationStrategy(
            IGeoProjection projection,
            IBeaconWeightCalculator weightCalculator,
            ILinearSystemSolver solver,
            IPositionFallbackEstimator fallbackEstimator,
            IAccuracyCalculator accuracyCalculator)
        {
            _projection = projection;
            _weightCalculator = weightCalculator;
            _solver = solver;
            _fallbackEstimator = fallbackEstimator;
            _accuracyCalculator = accuracyCalculator;
        }

        public bool CanEstimate(int beaconCount)
        {
            return beaconCount >= 3;
        }

        public TrilaterationResult Estimate(IReadOnlyList<BeaconSample> beacons)
        {
            var origin = new GeoPoint(beacons[0].Latitude, beacons[0].Longitude);

            var localPoints = beacons
                .Select(beacon =>
                {
                    var coordinate = _projection.ToLocalCoordinate(beacon, origin);
                    var weight = _weightCalculator.Calculate(beacon.EstimatedDistanceMeters);

                    return new LocalPoint(
                        coordinate.X,
                        coordinate.Y,
                        beacon.EstimatedDistanceMeters,
                        weight);
                })
                .ToArray();

            var linearSystem = LeastSquaresLinearSystemBuilder.Build(localPoints);

            if (!_solver.TrySolve(linearSystem, out var estimatedCoordinate))
                return _fallbackEstimator.Estimate(beacons);

            var estimatedGeoPoint = _projection.ToGeoPoint(estimatedCoordinate, origin);

            return new TrilaterationResult
            {
                Latitude = estimatedGeoPoint.Latitude,
                Longitude = estimatedGeoPoint.Longitude,
                BeaconsUsed = beacons.Count,
                AccuracyMeters = _accuracyCalculator.Calculate(localPoints, estimatedCoordinate),
                Method = EstimationMethod.Trilateration
            };
        }
    }
}
