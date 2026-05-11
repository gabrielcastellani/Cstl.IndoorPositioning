using Cstl.IndoorPositioning.Abstractions.Models;
using Cstl.IndoorPositioning.Abstractions.Services;
using Cstl.IndoorPositioning.Strategies;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cstl.IndoorPositioning
{
    internal sealed class DefaultTrilaterationEngine : ITrilaterationEngine
    {
        private readonly IReadOnlyCollection<IPositionEstimationStrategy> _strategies;

        public DefaultTrilaterationEngine(IEnumerable<IPositionEstimationStrategy> strategies)
        {
            _strategies = strategies.ToArray();
        }

        public TrilaterationResult Estimate(IReadOnlyList<BeaconSample> beacons)
        {
            Validate(beacons);

            var strategy = _strategies.FirstOrDefault(x => x.CanEstimate(beacons.Count));

            if (strategy is null)
                throw new InvalidOperationException(
                    message: $"No estimation strategy found for {beacons.Count} beacons.");

            return strategy.Estimate(beacons);
        }

        private static void Validate(IReadOnlyList<BeaconSample>? beacons)
        {
            if (beacons is null)
                throw new ArgumentNullException(nameof(beacons));

            if (beacons.Count == 0)
                throw new ArgumentException("At least one beacon is required.", nameof(beacons));

            if (beacons.Any(x => x.EstimatedDistanceMeters < 0))
                throw new ArgumentException("Beacon distance cannot be negative.", nameof(beacons));
        }
    }
}
