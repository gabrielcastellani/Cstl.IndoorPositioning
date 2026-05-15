using Cstl.IndoorPositioning.Abstractions.Enums;
using Cstl.IndoorPositioning.Abstractions.Models;
using Cstl.IndoorPositioning.Algorithms.Distance;

namespace Cstl.IndoorPositioning.Tests
{
    /// <summary>
    /// Unit tests for <see cref="LogDistanceBeaconDistanceCalculator" />.
    /// </summary>
    public sealed class LogDistanceCalculatorTests
    {
        [Fact]
        public void CalculateDistance_WhenRssiEqualsTxPower_ReturnsOneMeter()
        {
            var calculator = new LogDistanceBeaconDistanceCalculator();

            var distance = calculator.CalculateDistance(rssi: -59, txPower: -59);

            Assert.Equal(1.0, distance, precision: 5);
        }

        [Fact]
        public void CalculateDistance_StrongerSignal_ReturnsSmallerDistance()
        {
            var calculator = new LogDistanceBeaconDistanceCalculator();

            var near = calculator.CalculateDistance(rssi: -40, txPower: -59);
            var far = calculator.CalculateDistance(rssi: -80, txPower: -59);

            Assert.True(near < far);
        }

        [Fact]
        public void CalculateDistance_FromReading_UsesReadingTxPower()
        {
            var calculator = new LogDistanceBeaconDistanceCalculator();
            var reading = new BeaconReading("AA:BB:CC:DD:EE:01", rssi: -59, txPower: -59);

            var distance = calculator.CalculateDistance(reading);

            Assert.Equal(1.0, distance, precision: 5);
        }

        [Fact]
        public void CalculateDistance_NullTxPower_UsesConfiguredDefault()
        {
            var calculator = new LogDistanceBeaconDistanceCalculator(defaultTxPower: -60);

            var withNull = calculator.CalculateDistance(rssi: -60, txPower: null);
            var withExplicitDefault = calculator.CalculateDistance(rssi: -60, txPower: -60);

            Assert.Equal(withExplicitDefault, withNull, precision: 10);
        }

        [Fact]
        public void CalculateExpectedRssi_IsInverseOfCalculateDistance()
        {
            var calculator = new LogDistanceBeaconDistanceCalculator();
            const int txPower = -59;
            const double expectedDistance = 5.0;

            var rssi = calculator.CalculateExpectedRssi(expectedDistance, txPower);
            var recoveredDistance = calculator.CalculateDistance((int)Math.Round(rssi), txPower);

            Assert.Equal(expectedDistance, recoveredDistance, precision: 0);
        }

        [Theory]
        [InlineData(EnvironmentProfile.FreeSpace, 2.0)]
        [InlineData(EnvironmentProfile.OpenSpace, 2.5)]
        [InlineData(EnvironmentProfile.Indoor, 3.0)]
        [InlineData(EnvironmentProfile.Obstructed, 3.5)]
        [InlineData(EnvironmentProfile.Industrial, 4.0)]
        public void ForProfile_UsesExpectedPathLossExponent(EnvironmentProfile profile, double expected)
        {
            var calculator = LogDistanceBeaconDistanceCalculator.ForProfile(profile);

            Assert.Equal(expected, calculator.PathLossExponent, precision: 5);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Constructor_InvalidPathLossExponent_Throws(double pathLossExponent)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new LogDistanceBeaconDistanceCalculator(pathLossExponent));
        }

        [Fact]
        public void Constructor_InvalidDefaultTxPower_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new LogDistanceBeaconDistanceCalculator(defaultTxPower: 0));
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(0.009)]
        public void CalculateExpectedRssi_DistanceBelowMinimum_Throws(double distanceMeters)
        {
            var calculator = new LogDistanceBeaconDistanceCalculator();

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                calculator.CalculateExpectedRssi(distanceMeters));
        }
    }
}
