using Cstl.IndoorPositioning.Abstractions.Enums;
using Cstl.IndoorPositioning.Abstractions.Models;
using Cstl.IndoorPositioning.Abstractions.Services;

namespace Cstl.IndoorPositioning.Tests
{
    public class TrilaterationEngineTests
    {
        private const double TargetLatitude = -23.5505;
        private const double TargetLongitude = -46.6326;

        private static BeaconSample MakeSample(
            double latitude,
            double longitude,
            double distanceMeters)
        {
            return new BeaconSample
            {
                MAC = Guid.NewGuid().ToString(),
                Latitude = latitude,
                Longitude = longitude,
                EstimatedDistanceMeters = distanceMeters
            };
        }

        [Fact]
        public void Estimate_WhenThereIsOneBeacon_ShouldReturnProximityAndBeaconPosition()
        {
            // Arrange
            var beacon = MakeSample(
                latitude: -23.5505,
                longitude: -46.6333,
                distanceMeters: 5.0);

            // Act
            var result = TrilaterationEngine.Estimate(new[] { beacon });

            // Assert
            Assert.Equal(EstimationMethod.Proximity, result.Method);
            Assert.Equal(beacon.Latitude, result.Latitude);
            Assert.Equal(beacon.Longitude, result.Longitude);
            Assert.Equal(1, result.BeaconsUsed);
            Assert.Equal(beacon.EstimatedDistanceMeters, result.AccuracyMeters);
        }

        [Fact]
        public void Estimate_WhenThereAreTwoBeaconsWithSameDistance_ShouldReturnBilaterationAndMiddlePoint()
        {
            // Arrange
            var firstBeacon = MakeSample(
                latitude: -23.5505,
                longitude: -46.6333,
                distanceMeters: 10.0);

            var secondBeacon = MakeSample(
                latitude: -23.5505,
                longitude: -46.6320,
                distanceMeters: 10.0);

            // Act
            var result = TrilaterationEngine.Estimate(new[] { firstBeacon, secondBeacon });

            // Assert
            Assert.Equal(EstimationMethod.Bilateration, result.Method);
            Assert.Equal(2, result.BeaconsUsed);

            Assert.Equal(
                expected: (firstBeacon.Latitude + secondBeacon.Latitude) / 2.0,
                actual: result.Latitude,
                precision: 6);

            Assert.Equal(
                expected: (firstBeacon.Longitude + secondBeacon.Longitude) / 2.0,
                actual: result.Longitude,
                precision: 6);
        }

        [Fact]
        public void Estimate_WhenThereAreTwoBeaconsWithDifferentDistances_ShouldGiveMoreWeightToCloserBeacon()
        {
            // Arrange
            var farBeacon = MakeSample(
                latitude: -23.5505,
                longitude: -46.6333,
                distanceMeters: 50.0);

            var nearBeacon = MakeSample(
                latitude: -23.5505,
                longitude: -46.6320,
                distanceMeters: 5.0);

            // Act
            var result = TrilaterationEngine.Estimate(new[] { farBeacon, nearBeacon });

            // Assert
            Assert.Equal(EstimationMethod.Bilateration, result.Method);
            Assert.Equal(2, result.BeaconsUsed);

            var distanceToNearBeacon = HaversineMeters(
                result.Latitude,
                result.Longitude,
                nearBeacon.Latitude,
                nearBeacon.Longitude);

            var distanceToFarBeacon = HaversineMeters(
                result.Latitude,
                result.Longitude,
                farBeacon.Latitude,
                farBeacon.Longitude);

            Assert.True(
                distanceToNearBeacon < distanceToFarBeacon,
                "The estimated position should be closer to the beacon with the smaller estimated distance.");
        }

        [Fact]
        public void Estimate_WhenThereAreThreeBeacons_ShouldReturnTrilaterationWithinExpectedError()
        {
            // Arrange
            var samples = new[]
            {
                MakeSample(
                    latitude: -23.5505,
                    longitude: -46.6333,
                    distanceMeters: DistanceToTarget(-23.5505, -46.6333)),

                MakeSample(
                    latitude: -23.5505,
                    longitude: -46.6320,
                    distanceMeters: DistanceToTarget(-23.5505, -46.6320)),

                MakeSample(
                    latitude: -23.5495,
                    longitude: -46.6326,
                    distanceMeters: DistanceToTarget(-23.5495, -46.6326))
            };

            // Act
            var result = TrilaterationEngine.Estimate(samples);

            // Assert
            Assert.Equal(EstimationMethod.Trilateration, result.Method);
            Assert.Equal(3, result.BeaconsUsed);

            AssertPositionWithinMeters(
                actualLatitude: result.Latitude,
                actualLongitude: result.Longitude,
                expectedLatitude: TargetLatitude,
                expectedLongitude: TargetLongitude,
                maxErrorMeters: 5.0);
        }

        [Fact]
        public void Estimate_WhenThereAreFourBeacons_ShouldUseAllBeaconsAndReturnPositionWithinExpectedError()
        {
            // Arrange
            var samples = new[]
            {
                MakeSample(
                    latitude: -23.5505,
                    longitude: -46.6333,
                    distanceMeters: DistanceToTarget(-23.5505, -46.6333)),

                MakeSample(
                    latitude: -23.5505,
                    longitude: -46.6320,
                    distanceMeters: DistanceToTarget(-23.5505, -46.6320)),

                MakeSample(
                    latitude: -23.5495,
                    longitude: -46.6326,
                    distanceMeters: DistanceToTarget(-23.5495, -46.6326)),

                MakeSample(
                    latitude: -23.5515,
                    longitude: -46.6326,
                    distanceMeters: DistanceToTarget(-23.5515, -46.6326))
            };

            // Act
            var result = TrilaterationEngine.Estimate(samples);

            // Assert
            Assert.Equal(EstimationMethod.Trilateration, result.Method);
            Assert.Equal(4, result.BeaconsUsed);

            AssertPositionWithinMeters(
                actualLatitude: result.Latitude,
                actualLongitude: result.Longitude,
                expectedLatitude: TargetLatitude,
                expectedLongitude: TargetLongitude,
                maxErrorMeters: 5.0);
        }

        [Fact]
        public void Estimate_WhenBeaconListIsEmpty_ShouldThrowArgumentException()
        {
            // Act
            Action act = () => TrilaterationEngine.Estimate(Array.Empty<BeaconSample>());

            // Assert
            Assert.Throws<ArgumentException>(act);
        }

        [Fact]
        public void Estimate_WhenBeaconListIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            IReadOnlyList<BeaconSample> beacons = null;

            // Act
            Action act = () => TrilaterationEngine.Estimate(beacons);

            // Assert
            Assert.Throws<ArgumentNullException>(act);
        }

        [Fact]
        public void Estimate_WhenBeaconHasNegativeDistance_ShouldThrowArgumentException()
        {
            // Arrange
            var beacon = MakeSample(
                latitude: -23.5505,
                longitude: -46.6333,
                distanceMeters: -1.0);

            // Act
            Action act = () => TrilaterationEngine.Estimate(new[] { beacon });

            // Assert
            Assert.Throws<ArgumentException>(act);
        }

        [Fact]
        public void CreateDefault_WhenCalled_ShouldReturnUsableEngine()
        {
            // Arrange
            ITrilaterationEngine engine = TrilaterationEngineFactory.CreateDefault();

            var beacon = MakeSample(
                latitude: -23.5505,
                longitude: -46.6333,
                distanceMeters: 5.0);

            // Act
            var result = engine.Estimate(new[] { beacon });

            // Assert
            Assert.NotNull(engine);
            Assert.Equal(EstimationMethod.Proximity, result.Method);
            Assert.Equal(1, result.BeaconsUsed);
        }

        private static double DistanceToTarget(double latitude, double longitude)
        {
            return HaversineMeters(
                latitude,
                longitude,
                TargetLatitude,
                TargetLongitude);
        }

        private static void AssertPositionWithinMeters(
            double actualLatitude,
            double actualLongitude,
            double expectedLatitude,
            double expectedLongitude,
            double maxErrorMeters)
        {
            var errorMeters = HaversineMeters(
                actualLatitude,
                actualLongitude,
                expectedLatitude,
                expectedLongitude);

            Assert.True(
                errorMeters < maxErrorMeters,
                $"Error was {errorMeters:F2}m. Expected less than {maxErrorMeters:F2}m.");
        }

        private static double HaversineMeters(
            double latitude1,
            double longitude1,
            double latitude2,
            double longitude2)
        {
            const double earthRadiusMeters = 6_371_000.0;

            var deltaLatitude = ToRadians(latitude2 - latitude1);
            var deltaLongitude = ToRadians(longitude2 - longitude1);

            var latitude1Radians = ToRadians(latitude1);
            var latitude2Radians = ToRadians(latitude2);

            var a =
                Math.Sin(deltaLatitude / 2.0) * Math.Sin(deltaLatitude / 2.0) +
                Math.Cos(latitude1Radians) *
                Math.Cos(latitude2Radians) *
                Math.Sin(deltaLongitude / 2.0) *
                Math.Sin(deltaLongitude / 2.0);

            var c = 2.0 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1.0 - a));

            return earthRadiusMeters * c;
        }

        private static double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }
    }
}
