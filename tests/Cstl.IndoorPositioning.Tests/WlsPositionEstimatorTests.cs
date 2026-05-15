using Cstl.IndoorPositioning.Abstractions.Enums;
using Cstl.IndoorPositioning.Abstractions.Models;
using Cstl.IndoorPositioning.Algorithms;

namespace Cstl.IndoorPositioning.Tests
{
    /// <summary>
    /// Unit tests for <see cref="WeightedLeastSquaresPositionEstimator" />.
    /// </summary>
    public sealed class WlsPositionEstimatorTests
    {
        private readonly WeightedLeastSquaresPositionEstimator _estimator = new();

        private static BeaconSample MakeSample(double latitude, double longitude, int rssi, int txPower = -59)
        {
            var distance = BeaconDistanceCalculator.Calculate(rssi, txPower == 0 ? null : txPower);
            return new BeaconSample(
                NewMac(),
                latitude,
                longitude,
                rssi,
                txPower,
                distance);
        }

        private static BeaconSample MakeSampleAtDistance(
            double latitude,
            double longitude,
            double distanceMeters,
            int txPower = -59)
        {
            var rssi = (int)Math.Round(BeaconDistanceCalculator.DistanceToRssi(distanceMeters, txPower));
            return new BeaconSample(
                NewMac(),
                latitude,
                longitude,
                rssi,
                txPower,
                distanceMeters);
        }

        [Fact]
        public void Estimate_OneBeacon_ReturnsProximityAtBeaconPosition()
        {
            var sample = MakeSample(-23.5505, -46.6333, rssi: -60);

            var result = _estimator.Estimate(new[] { sample });

            Assert.Equal(EstimationMethod.Proximity, result.Method);
            Assert.Equal(1, result.BeaconsUsed);
            Assert.Equal(sample.Latitude, result.Latitude);
            Assert.Equal(sample.Longitude, result.Longitude);
        }

        [Fact]
        public void Estimate_TwoBeaconsEqualRssi_ReturnsMidpoint()
        {
            var first = MakeSample(-23.5505, -46.6333, rssi: -60);
            var second = MakeSample(-23.5505, -46.6320, rssi: -60);

            var result = _estimator.Estimate(new[] { first, second });

            Assert.Equal(EstimationMethod.Bilateration, result.Method);
            Assert.Equal(2, result.BeaconsUsed);
            Assert.Equal((first.Latitude + second.Latitude) / 2, result.Latitude, precision: 6);
            Assert.Equal((first.Longitude + second.Longitude) / 2, result.Longitude, precision: 6);
        }

        [Fact]
        public void Estimate_TwoBeacons_StrongerRssiBeaconHasMoreWeight()
        {
            var weak = MakeSample(-23.5505, -46.6333, rssi: -80);
            var strong = MakeSample(-23.5505, -46.6320, rssi: -40);

            var result = _estimator.Estimate(new[] { weak, strong });

            var distanceToStrong = Math.Abs(result.Longitude - strong.Longitude);
            var distanceToWeak = Math.Abs(result.Longitude - weak.Longitude);
            Assert.True(distanceToStrong < distanceToWeak, "Beacon with stronger RSSI must carry more weight.");
        }

        [Fact]
        public void Estimate_ThreeBeacons_ReturnsTrilaterationWithinTolerance()
        {
            const double targetLatitude = -23.5505;
            const double targetLongitude = -46.6326;

            var samples = new[]
            {
            MakeSampleAtDistance(-23.5505, -46.6333, HaversineMeters(-23.5505, -46.6333, targetLatitude, targetLongitude)),
            MakeSampleAtDistance(-23.5505, -46.6320, HaversineMeters(-23.5505, -46.6320, targetLatitude, targetLongitude)),
            MakeSampleAtDistance(-23.5495, -46.6326, HaversineMeters(-23.5495, -46.6326, targetLatitude, targetLongitude))
        };

            var result = _estimator.Estimate(samples);

            Assert.Equal(EstimationMethod.Trilateration, result.Method);
            Assert.Equal(3, result.BeaconsUsed);
            Assert.True(HaversineMeters(result.Latitude, result.Longitude, targetLatitude, targetLongitude) < 5.0);
        }

        [Fact]
        public void Estimate_FourBeacons_UsesAllBeacons()
        {
            const double targetLatitude = -23.5505;
            const double targetLongitude = -46.6326;

            var samples = new[]
            {
            MakeSampleAtDistance(-23.5505, -46.6333, HaversineMeters(-23.5505, -46.6333, targetLatitude, targetLongitude)),
            MakeSampleAtDistance(-23.5505, -46.6320, HaversineMeters(-23.5505, -46.6320, targetLatitude, targetLongitude)),
            MakeSampleAtDistance(-23.5495, -46.6326, HaversineMeters(-23.5495, -46.6326, targetLatitude, targetLongitude)),
            MakeSampleAtDistance(-23.5515, -46.6326, HaversineMeters(-23.5515, -46.6326, targetLatitude, targetLongitude))
        };

            var result = _estimator.Estimate(samples);

            Assert.Equal(EstimationMethod.Trilateration, result.Method);
            Assert.Equal(4, result.BeaconsUsed);
            Assert.True(HaversineMeters(result.Latitude, result.Longitude, targetLatitude, targetLongitude) < 5.0);
        }

        [Fact]
        public void Estimate_StrongRssiBeaconPullsResultCloser()
        {
            var strong = MakeSample(-23.5500, -46.6326, rssi: -45);
            var weak1 = MakeSample(-23.5510, -46.6333, rssi: -75);
            var weak2 = MakeSample(-23.5510, -46.6320, rssi: -75);
            var weak3 = MakeSample(-23.5495, -46.6310, rssi: -75);

            var result = _estimator.Estimate(new[] { strong, weak1, weak2, weak3 });

            var distanceToStrong = HaversineMeters(result.Latitude, result.Longitude, strong.Latitude, strong.Longitude);
            var centroidLatitude = (strong.Latitude + weak1.Latitude + weak2.Latitude + weak3.Latitude) / 4.0;
            var centroidLongitude = (strong.Longitude + weak1.Longitude + weak2.Longitude + weak3.Longitude) / 4.0;
            var distanceToCentroid = HaversineMeters(result.Latitude, result.Longitude, centroidLatitude, centroidLongitude);

            Assert.True(
                distanceToStrong < distanceToCentroid,
                $"Result ({distanceToStrong:F1}m from strong beacon) should be closer to strong beacon than centroid ({distanceToCentroid:F1}m).");
        }

        [Fact]
        public void Estimate_CollinearBeacons_FallsBackToWeightedCentroid()
        {
            var first = MakeSample(-23.5505, -46.6333, rssi: -65);
            var second = MakeSample(-23.5505, -46.6320, rssi: -65);
            var third = MakeSample(-23.5505, -46.6310, rssi: -65);

            var result = _estimator.Estimate(new[] { first, second, third });

            Assert.Equal(EstimationMethod.WeightedCentroid, result.Method);
            Assert.Equal(3, result.BeaconsUsed);
        }

        [Fact]
        public void Estimate_EqualRssi_ThreeBeacons_ResultNearCentroid()
        {
            var first = MakeSample(-23.5505, -46.6333, rssi: -65);
            var second = MakeSample(-23.5505, -46.6320, rssi: -65);
            var third = MakeSample(-23.5495, -46.6326, rssi: -65);

            var centroidLatitude = (first.Latitude + second.Latitude + third.Latitude) / 3.0;
            var centroidLongitude = (first.Longitude + second.Longitude + third.Longitude) / 3.0;

            var result = _estimator.Estimate(new[] { first, second, third });
            var errorMeters = HaversineMeters(result.Latitude, result.Longitude, centroidLatitude, centroidLongitude);

            Assert.True(errorMeters < 20.0, $"Expected result near centroid; error = {errorMeters:F2} m.");
        }

        [Fact]
        public void Estimate_EmptyList_Throws()
        {
            Assert.Throws<ArgumentException>(() => _estimator.Estimate(Array.Empty<BeaconSample>()));
        }

        private static string NewMac() => Guid.NewGuid().ToString("N")[..12];

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
    }
}
