using Cstl.IndoorPositioning.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cstl.IndoorPositioning
{
    public sealed class BeaconLocator
    {
        private readonly double _pathLossExponent;

        public BeaconLocator(double pathLossExponent = BeaconDistanceCalculator.DefaultPathLossExponent)
        {
            if (pathLossExponent <= 0)
                throw new ArgumentOutOfRangeException(nameof(pathLossExponent), "Path loss exponent must be positive.");

            _pathLossExponent = pathLossExponent;
        }

        public TrilaterationResult Locate(IEnumerable<BeaconReading> readings, IEnumerable<BeaconAnchor> anchors)
        {
            var anchorMap = BuildAnchorMap(anchors);
            var samples = BuildSamples(readings, anchorMap);

            GuardSamplesNotEmpty(samples);

            return TrilaterationEngine.Estimate(samples);
        }

        public TrilaterationResult LocateFromSamples(IEnumerable<BeaconSample> samples)
        {
            var ordered = samples.OrderBy(s => s.EstimatedDistanceMeters).ToList();

            GuardSamplesNotEmpty(ordered);

            return TrilaterationEngine.Estimate(ordered);
        }

        private static Dictionary<string, BeaconAnchor> BuildAnchorMap(IEnumerable<BeaconAnchor> anchors)
        {
            return anchors.ToDictionary(a => a.MAC, StringComparer.OrdinalIgnoreCase);
        }

        private List<BeaconSample> BuildSamples(
            IEnumerable<BeaconReading> readings,
            Dictionary<string, BeaconAnchor> anchorMap)
        {
            return readings
                .Where(r => anchorMap.ContainsKey(r.MAC))
                .Select(r => BuildSample(r, anchorMap[r.MAC]))
                .OrderBy(s => s.EstimatedDistanceMeters)
                .ToList();
        }

        private BeaconSample BuildSample(BeaconReading reading, BeaconAnchor anchor)
        {
            var sample = BeaconSample.From(reading, anchor);
            sample.EstimatedDistanceMeters = BeaconDistanceCalculator.Calculate(reading.RSSI, reading.TxPower, _pathLossExponent);
            return sample;
        }

        private static void GuardSamplesNotEmpty(IReadOnlyCollection<BeaconSample> samples)
        {
            if (samples.Count == 0)
                throw new ArgumentException("No beacon readings could be matched to known anchors. Check the MAC addresses.");
        }
    }
}
