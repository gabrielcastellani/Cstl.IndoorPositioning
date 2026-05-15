using Cstl.IndoorPositioning.Abstractions.Enums;

namespace Cstl.IndoorPositioning.Tests
{
    /// <summary>
    /// Unit tests for the static <see cref="BeaconDistanceCalculator" /> facade.
    /// </summary>
    public sealed class BeaconDistanceCalculatorTests
    {
        [Fact]
        public void Calculate_WhenRssiEqualsTxPower_ReturnsOneMeter()
        {
            var distance = BeaconDistanceCalculator.Calculate(rssi: -59, txPower: -59);

            Assert.Equal(1.0, distance, precision: 5);
        }

        [Fact]
        public void Calculate_StrongerSignal_ReturnsSmallerDistance()
        {
            var near = BeaconDistanceCalculator.Calculate(rssi: -40, txPower: -59);
            var far = BeaconDistanceCalculator.Calculate(rssi: -80, txPower: -59);

            Assert.True(near < far);
        }

        [Fact]
        public void Calculate_WeakerSignal_ReturnsLargerDistance()
        {
            var distance3m = BeaconDistanceCalculator.Calculate(rssi: -69, txPower: -59, pathLossExponent: 3.0);
            var distance10m = BeaconDistanceCalculator.Calculate(rssi: -89, txPower: -59, pathLossExponent: 3.0);

            Assert.True(distance10m > distance3m);
        }

        [Fact]
        public void Calculate_MissingTxPower_UsesDefault()
        {
            var withNull = BeaconDistanceCalculator.Calculate(rssi: -65, txPower: null);
            var withDefault = BeaconDistanceCalculator.Calculate(rssi: -65, txPower: BeaconDistanceCalculator.DefaultTxPower);

            Assert.Equal(withDefault, withNull, precision: 10);
        }

        [Fact]
        public void DistanceToRssi_IsInverseOfCalculate()
        {
            const int txPower = -59;
            const double expectedDistance = 5.0;

            var rssi = BeaconDistanceCalculator.DistanceToRssi(expectedDistance, txPower);
            var recoveredDistance = BeaconDistanceCalculator.Calculate((int)Math.Round(rssi), txPower);

            Assert.Equal(expectedDistance, recoveredDistance, precision: 0);
        }

        [Theory]
        [InlineData(-40, -59, 3.0)]
        [InlineData(-59, -59, 3.0)]
        [InlineData(-89, -59, 3.0)]
        public void Calculate_AlwaysReturnsPositive(int rssi, int txPower, double pathLossExponent)
        {
            var distance = BeaconDistanceCalculator.Calculate(rssi, txPower, pathLossExponent);

            Assert.True(distance > 0);
        }

        [Theory]
        [InlineData(EnvironmentProfile.FreeSpace, 2.0)]
        [InlineData(EnvironmentProfile.OpenSpace, 2.5)]
        [InlineData(EnvironmentProfile.Indoor, 3.0)]
        [InlineData(EnvironmentProfile.Obstructed, 3.5)]
        [InlineData(EnvironmentProfile.Industrial, 4.0)]
        public void PathLossExponentFor_ReturnsExpectedValue(EnvironmentProfile profile, double expected)
        {
            var actual = BeaconDistanceCalculator.PathLossExponentFor(profile);

            Assert.Equal(expected, actual, precision: 5);
        }

        [Fact]
        public void Calculate_IndustrialProfile_EstimatesSmallerDistanceThanIndoorForSameRssi()
        {
            var industrial = BeaconDistanceCalculator.Calculate(-70, -59, EnvironmentProfile.Industrial);
            var indoor = BeaconDistanceCalculator.Calculate(-70, -59, EnvironmentProfile.Indoor);

            Assert.True(industrial < indoor);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Calculate_InvalidPathLossExponent_Throws(double pathLossExponent)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                BeaconDistanceCalculator.Calculate(-50, -59, pathLossExponent));
        }
    }
}
