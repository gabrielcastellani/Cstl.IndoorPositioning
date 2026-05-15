using Cstl.IndoorPositioning.Abstractions.Configuration;
using Cstl.IndoorPositioning.Abstractions.Enums;
using Cstl.IndoorPositioning.Abstractions.Models;
using Cstl.IndoorPositioning.Algorithms;

namespace Cstl.IndoorPositioning.Tests
{
    /// <summary>
    /// Unit tests for <see cref="KalmanPositionSmoother" />.
    /// </summary>
    public sealed class KalmanPositionSmootherTests
    {
        [Fact]
        public void Smooth_FirstReading_ReturnsRawResultWithoutSmoothing()
        {
            var smoother = new KalmanPositionSmoother();
            var raw = new TrilaterationResult(-23.5505, -46.6333, 1, 2.0, EstimationMethod.Proximity);

            var result = smoother.Smooth(raw);

            Assert.Same(raw, result);
        }

        [Fact]
        public void Smooth_SecondReading_ReturnsSmoothedValueBetweenPreviousAndCurrent()
        {
            var smoother = new KalmanPositionSmoother();
            var first = new TrilaterationResult(-23.5505, -46.6333, 3, 1.0, EstimationMethod.Trilateration);
            var second = new TrilaterationResult(-23.5405, -46.6233, 3, 1.0, EstimationMethod.Trilateration);

            smoother.Smooth(first);
            var result = smoother.Smooth(second);

            Assert.InRange(result.Latitude, first.Latitude, second.Latitude);
            Assert.InRange(result.Longitude, first.Longitude, second.Longitude);
            Assert.NotEqual(second.Latitude, result.Latitude, precision: 10);
            Assert.NotEqual(second.Longitude, result.Longitude, precision: 10);
            Assert.Equal(second.Method, result.Method);
            Assert.Equal(second.BeaconsUsed, result.BeaconsUsed);
            Assert.Equal(second.AccuracyMeters, result.AccuracyMeters);
        }

        [Fact]
        public void Reset_ClearsState_NextReadingIsReturnedWithoutSmoothing()
        {
            var smoother = new KalmanPositionSmoother();
            var first = new TrilaterationResult(-23.5505, -46.6333, 3, 1.0, EstimationMethod.Trilateration);
            var second = new TrilaterationResult(-23.5405, -46.6233, 3, 1.0, EstimationMethod.Trilateration);

            smoother.Smooth(first);
            smoother.Smooth(second);
            smoother.Reset();

            var result = smoother.Smooth(second);

            Assert.Same(second, result);
        }

        [Theory]
        [InlineData(0, 1e-3, 1.0)]
        [InlineData(1e-5, 0, 1.0)]
        [InlineData(1e-5, 1e-3, 0)]
        public void Constructor_InvalidOptions_Throws(
            double processNoise,
            double measurementNoise,
            double initialErrorCovariance)
        {
            var options = new KalmanSmoothingOptions
            {
                ProcessNoise = processNoise,
                MeasurementNoise = measurementNoise,
                InitialErrorCovariance = initialErrorCovariance
            };

            Assert.Throws<ArgumentOutOfRangeException>(() => new KalmanPositionSmoother(options));
        }

        [Fact]
        public void BeaconLocator_ResetSmoothing_ClearsKalmanState()
        {
            var anchors = new[]
            {
                new BeaconAnchor("AA:BB:CC:DD:EE:01", -23.5505, -46.6333)
            };
            var reading = new[]
            {
                new BeaconReading("AA:BB:CC:DD:EE:01", rssi: -65, txPower: -59)
            };
            var locator = new BeaconLocator();

            locator.Locate(reading, anchors);
            locator.ResetSmoothing();
            var result = locator.Locate(reading, anchors);

            Assert.Equal(anchors[0].Latitude, result.Latitude);
            Assert.Equal(anchors[0].Longitude, result.Longitude);
            Assert.Equal(EstimationMethod.Proximity, result.Method);
        }

        [Fact]
        public void BeaconLocator_InvalidPathLossExponent_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new BeaconLocator(new IndoorPositioningOptions { PathLossExponent = 0 }));

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new BeaconLocator(new IndoorPositioningOptions { PathLossExponent = -1 }));
        }
    }
}
