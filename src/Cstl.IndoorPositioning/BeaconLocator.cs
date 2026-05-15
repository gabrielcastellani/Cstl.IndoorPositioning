using Cstl.IndoorPositioning.Abstractions.Configuration;
using Cstl.IndoorPositioning.Abstractions.Enums;
using Cstl.IndoorPositioning.Abstractions.Exceptions;
using Cstl.IndoorPositioning.Abstractions.Interfaces;
using Cstl.IndoorPositioning.Abstractions.Models;
using Cstl.IndoorPositioning.Algorithms;
using Cstl.IndoorPositioning.Algorithms.Distance;

namespace Cstl.IndoorPositioning
{
    /// <summary>
    /// Complete indoor positioning pipeline: readings to distances, position estimation and optional smoothing.
    /// </summary>
    public sealed class BeaconLocator : IBeaconLocator
    {
        private readonly IBeaconDistanceCalculator _distanceCalculator;
        private readonly IPositionEstimator _positionEstimator;
        private readonly IPositionSmoother? _positionSmoother;

        /// <summary>
        /// Creates a locator using default indoor options.
        /// </summary>
        public BeaconLocator()
            : this(new IndoorPositioningOptions())
        { }

        /// <summary>
        /// Creates a locator using configured defaults.
        /// </summary>
        public BeaconLocator(IndoorPositioningOptions options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));

            options.Validate();

            _distanceCalculator = new LogDistanceBeaconDistanceCalculator(
                options.ResolvePathLossExponent(),
                options.DefaultTxPower);
            _positionEstimator = new WeightedLeastSquaresPositionEstimator();
            _positionSmoother = options.EnableSmoothing
                ? new KalmanPositionSmoother(options.Smoothing)
                : null;
        }

        /// <summary>
        /// Creates a locator with explicit dependencies.
        /// </summary>
        public BeaconLocator(
            IBeaconDistanceCalculator distanceCalculator,
            IPositionEstimator positionEstimator,
            IPositionSmoother? positionSmoother = null)
        {
            _distanceCalculator = distanceCalculator ?? throw new ArgumentNullException(nameof(distanceCalculator));
            _positionEstimator = positionEstimator ?? throw new ArgumentNullException(nameof(positionEstimator));
            _positionSmoother = positionSmoother;
        }

        /// <summary>
        /// Creates a locator using a predefined environment profile.
        /// </summary>
        public static BeaconLocator ForProfile(EnvironmentProfile profile)
        {
            return new BeaconLocator(new IndoorPositioningOptions
            {
                EnvironmentProfile = profile
            });
        }

        /// <inheritdoc />
        public TrilaterationResult Locate(IEnumerable<BeaconReading> readings, IEnumerable<BeaconAnchor> anchors)
        {
            if (readings is null)
                throw new ArgumentNullException(nameof(readings));
            if (anchors is null)
                throw new ArgumentNullException(nameof(anchors));

            var anchorMap = BuildAnchorMap(anchors);
            var samples = CreateSamples(readings, anchorMap);

            if (samples.Count == 0)
                throw new NoMatchingBeaconsException();

            var rawEstimate = _positionEstimator.Estimate(samples);
            return _positionSmoother?.Smooth(rawEstimate) ?? rawEstimate;
        }

        /// <inheritdoc />
        public void ResetSmoothing()
        {
            _positionSmoother?.Reset();
        }

        private static Dictionary<MacAddress, BeaconAnchor> BuildAnchorMap(IEnumerable<BeaconAnchor> anchors)
        {
            var map = new Dictionary<MacAddress, BeaconAnchor>();

            foreach (var anchor in anchors)
            {
                if (anchor is null)
                    continue;

                if (map.ContainsKey(anchor.Mac))
                    throw new ArgumentException($"Duplicate anchor MAC address: {anchor.Mac}.", nameof(anchors));

                map.Add(anchor.Mac, anchor);
            }

            if (map.Count == 0)
                throw new ArgumentException("At least one anchor is required.", nameof(anchors));

            return map;
        }

        private IReadOnlyList<BeaconSample> CreateSamples(
            IEnumerable<BeaconReading> readings,
            IReadOnlyDictionary<MacAddress, BeaconAnchor> anchorMap)
        {
            var samples = new List<BeaconSample>();

            foreach (var reading in readings)
            {
                if (reading is null)
                    continue;

                if (!anchorMap.TryGetValue(reading.Mac, out var anchor))
                    continue;

                samples.Add(BeaconSample.From(reading, anchor, _distanceCalculator));
            }

            return samples;
        }
    }
}
