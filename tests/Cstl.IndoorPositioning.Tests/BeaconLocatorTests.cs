using Cstl.IndoorPositioning.Abstractions.Enums;
using Cstl.IndoorPositioning.Abstractions.Exceptions;
using Cstl.IndoorPositioning.Abstractions.Interfaces;
using Cstl.IndoorPositioning.Abstractions.Models;

namespace Cstl.IndoorPositioning.Tests
{
    /// <summary>
    /// Integration-style unit tests for <see cref="BeaconLocator" />.
    /// Exercises the full pipeline: RSSI reading → distance → trilateration → optional smoothing.
    /// </summary>
    public sealed class BeaconLocatorTests
    {
        private static readonly BeaconAnchor[] Anchors =
        {
        new("AA:BB:CC:DD:EE:01", -23.5505, -46.6333),
        new("AA:BB:CC:DD:EE:02", -23.5505, -46.6320),
        new("AA:BB:CC:DD:EE:03", -23.5495, -46.6326),
        new("AA:BB:CC:DD:EE:04", -23.5515, -46.6326)
    };

        private static int DistanceToRssi(double distanceMeters, int txPower = -59, double pathLossExponent = 3.0)
            => (int)Math.Round(BeaconDistanceCalculator.DistanceToRssi(distanceMeters, txPower, pathLossExponent));

        [Fact]
        public void Locate_ThreeBeacons_EstimatesPositionWithinTolerance()
        {
            const double targetLatitude = -23.5505;
            const double targetLongitude = -46.6326;

            var readings = new[]
            {
            new BeaconReading(
                "AA:BB:CC:DD:EE:01",
                DistanceToRssi(HaversineMeters(-23.5505, -46.6333, targetLatitude, targetLongitude)),
                txPower: -59),
            new BeaconReading(
                "AA:BB:CC:DD:EE:02",
                DistanceToRssi(HaversineMeters(-23.5505, -46.6320, targetLatitude, targetLongitude)),
                txPower: -59),
            new BeaconReading(
                "AA:BB:CC:DD:EE:03",
                DistanceToRssi(HaversineMeters(-23.5495, -46.6326, targetLatitude, targetLongitude)),
                txPower: -59)
        };

            var result = new BeaconLocator().Locate(readings, Anchors);

            Assert.Equal(EstimationMethod.Trilateration, result.Method);
            Assert.Equal(3, result.BeaconsUsed);

            var errorMeters = HaversineMeters(result.Latitude, result.Longitude, targetLatitude, targetLongitude);
            Assert.True(errorMeters < 10.0, $"Error {errorMeters:F2} m — expected < 10 m.");
        }

        [Fact]
        public void Locate_UnknownMac_IsIgnored()
        {
            var readings = new[]
            {
            new BeaconReading("AA:BB:CC:DD:EE:01", rssi: -65, txPower: -59),
            new BeaconReading("FF:FF:FF:FF:FF:FF", rssi: -50, txPower: -59)
        };

            var result = new BeaconLocator().Locate(readings, Anchors);

            Assert.Equal(EstimationMethod.Proximity, result.Method);
            Assert.Equal(1, result.BeaconsUsed);
        }

        [Fact]
        public void Locate_AllMacsUnknown_ThrowsNoMatchingBeaconsException()
        {
            var readings = new[]
            {
            new BeaconReading("FF:FF:FF:FF:FF:FF", rssi: -50, txPower: -59)
        };

            Assert.Throws<NoMatchingBeaconsException>(() => new BeaconLocator().Locate(readings, Anchors));
        }

        [Fact]
        public void Locate_MacIsCaseInsensitive()
        {
            var readings = new[]
            {
            new BeaconReading("aa:bb:cc:dd:ee:01", rssi: -65, txPower: -59)
        };

            var result = new BeaconLocator().Locate(readings, Anchors);

            Assert.Equal(1, result.BeaconsUsed);
            Assert.Equal(EstimationMethod.Proximity, result.Method);
        }

        [Fact]
        public void Locate_FourBeacons_UsesAllMatchedBeacons()
        {
            const double targetLatitude = -23.5505;
            const double targetLongitude = -46.6326;

            var readings = Anchors.Select(anchor => new BeaconReading(
                anchor.Mac.Value,
                DistanceToRssi(HaversineMeters(anchor.Latitude, anchor.Longitude, targetLatitude, targetLongitude)),
                txPower: -59)).ToArray();

            var result = new BeaconLocator().Locate(readings, Anchors);

            Assert.Equal(4, result.BeaconsUsed);
            Assert.Equal(EstimationMethod.Trilateration, result.Method);
        }

        [Fact]
        public void Locate_DuplicateAnchors_ThrowsArgumentException()
        {
            var anchors = new[]
            {
            new BeaconAnchor("AA:BB:CC:DD:EE:01", -23.5505, -46.6333),
            new BeaconAnchor("aa:bb:cc:dd:ee:01", -23.5506, -46.6334)
        };
            var readings = new[]
            {
            new BeaconReading("AA:BB:CC:DD:EE:01", rssi: -65, txPower: -59)
        };

            Assert.Throws<ArgumentException>(() => new BeaconLocator().Locate(readings, anchors));
        }


        [Fact]
        public void ForProfile_Industrial_EstimatesSmallerAccuracyThanIndoorForSameRssi()
        {
            var indoorLocator = BeaconLocator.ForProfile(EnvironmentProfile.Indoor);
            var industrialLocator = BeaconLocator.ForProfile(EnvironmentProfile.Industrial);
            var readings = new[]
            {
            new BeaconReading("AA:BB:CC:DD:EE:01", rssi: -70, txPower: -59)
        };

            var indoor = indoorLocator.Locate(readings, Anchors);
            var industrial = industrialLocator.Locate(readings, Anchors);

            Assert.Equal(EstimationMethod.Proximity, indoor.Method);
            Assert.Equal(EstimationMethod.Proximity, industrial.Method);
            Assert.True(industrial.AccuracyMeters < indoor.AccuracyMeters);
        }

        [Fact]
        public void Constructor_WithExplicitDependencies_UsesInjectedPipeline()
        {
            var expected = new TrilaterationResult(-23.5505, -46.6333, 1, 2.0, EstimationMethod.Proximity);
            var distanceCalculator = new FixedDistanceCalculator(distanceMeters: 2.0);
            var estimator = new FixedEstimator(expected);
            var smoother = new TrackingSmoother();
            var locator = new BeaconLocator(distanceCalculator, estimator, smoother);

            var result = locator.Locate(
                new[] { new BeaconReading("AA:BB:CC:DD:EE:01", rssi: -65, txPower: -59) },
                new[] { new BeaconAnchor("AA:BB:CC:DD:EE:01", -23.5505, -46.6333) });

            Assert.Same(expected, result);
            Assert.True(estimator.WasCalled);
            Assert.True(smoother.WasCalled);
        }

        private static double HaversineMeters(double latitude1, double longitude1, double latitude2, double longitude2)
        {
            const double earthRadiusMeters = 6_371_000.0;
            var deltaLatitude = (latitude2 - latitude1) * Math.PI / 180.0;
            var deltaLongitude = (longitude2 - longitude1) * Math.PI / 180.0;
            var a = Math.Sin(deltaLatitude / 2) * Math.Sin(deltaLatitude / 2)
                    + Math.Cos(latitude1 * Math.PI / 180.0) * Math.Cos(latitude2 * Math.PI / 180.0)
                    * Math.Sin(deltaLongitude / 2) * Math.Sin(deltaLongitude / 2);

            return earthRadiusMeters * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        }

        private sealed class FixedDistanceCalculator : IBeaconDistanceCalculator
        {
            private readonly double _distanceMeters;

            public FixedDistanceCalculator(double distanceMeters)
            {
                _distanceMeters = distanceMeters;
            }

            public double CalculateDistance(BeaconReading reading) => _distanceMeters;

            public double CalculateDistance(int rssi, int? txPower = null) => _distanceMeters;

            public double CalculateExpectedRssi(double distanceMeters, int? txPower = null) => -59;
        }

        private sealed class FixedEstimator : IPositionEstimator
        {
            private readonly TrilaterationResult _result;

            public FixedEstimator(TrilaterationResult result)
            {
                _result = result;
            }

            public bool WasCalled { get; private set; }

            public TrilaterationResult Estimate(IReadOnlyList<BeaconSample> samples)
            {
                WasCalled = true;
                Assert.Single(samples);
                Assert.Equal(2.0, samples[0].EstimatedDistanceMeters);
                return _result;
            }
        }

        private sealed class TrackingSmoother : IPositionSmoother
        {
            public bool WasCalled { get; private set; }

            public TrilaterationResult Smooth(TrilaterationResult raw)
            {
                WasCalled = true;
                return raw;
            }

            public void Reset()
            {
            }
        }
    }
}
