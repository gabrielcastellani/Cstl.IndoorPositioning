namespace Cstl.IndoorPositioning.Tests
{
    public class BeaconDistanceCalculatorTests
    {
        [Fact]
        public void Calculate_WhenRssiEqualsTxPower_Returns1Meter()
        {
            var dist = BeaconDistanceCalculator.Calculate(rssi: -59, txPower: -59);

            Assert.Equal(1.0, dist, precision: 5);
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
            var dist3m = BeaconDistanceCalculator.Calculate(rssi: -69, txPower: -59, pathLossExponent: 3.0);
            var dist10m = BeaconDistanceCalculator.Calculate(rssi: -89, txPower: -59, pathLossExponent: 3.0);

            Assert.True(dist10m > dist3m);
        }

        [Fact]
        public void DistanceToRssi_IsInverseOfCalculate()
        {
            var txPower = -59;
            var expectedDistance = 5.0;

            var rssi = BeaconDistanceCalculator.DistanceToRssi(expectedDistance, txPower);
            var recoveredDistance = BeaconDistanceCalculator.Calculate((int)Math.Round(rssi), txPower);

            Assert.Equal(expectedDistance, recoveredDistance, precision: 0); // ±0.5m por arredondamento do RSSI
        }

        [Fact]
        public void Calculate_InvalidPathLossExponent_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                BeaconDistanceCalculator.Calculate(-50, -59, pathLossExponent: 0));
        }

        [Theory]
        [InlineData(-40, -59, 3.0)]
        [InlineData(-59, -59, 3.0)]
        [InlineData(-89, -59, 3.0)]
        public void Calculate_AlwaysReturnsPositive(int rssi, int txPower, double n)
        {
            var dist = BeaconDistanceCalculator.Calculate(rssi, txPower, n);
            Assert.True(dist > 0);
        }
    }
}