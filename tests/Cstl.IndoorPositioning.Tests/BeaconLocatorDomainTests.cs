using Cstl.IndoorPositioning.Abstractions.Interfaces;
using Cstl.IndoorPositioning.Abstractions.Models;

namespace Cstl.IndoorPositioning.Tests
{
    // <summary>
    /// Unit tests for public domain/value objects from Cstl.IndoorPositioning.Abstractions.
    /// </summary>
    public sealed class BeaconLocatorDomainTests
    {
        [Theory]
        [InlineData("aa:bb:cc:dd:ee:ff", "AA:BB:CC:DD:EE:FF")]
        [InlineData("AA-BB-CC-DD-EE-FF", "AA:BB:CC:DD:EE:FF")]
        [InlineData("AABBCCDDEEFF", "AA:BB:CC:DD:EE:FF")]
        public void MacAddress_NormalizesSupportedFormats(string input, string expected)
        {
            var mac = new MacAddress(input);

            Assert.Equal(expected, mac.Value);
            Assert.Equal(expected, mac.ToString());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("AA:BB")]
        [InlineData("ZZ:BB:CC:DD:EE:FF")]
        public void MacAddress_InvalidValue_Throws(string? input)
        {
            Assert.Throws<ArgumentException>(() => new MacAddress(input!));
        }

        [Fact]
        public void MacAddress_IsCaseInsensitive()
        {
            var lower = new MacAddress("aa:bb:cc:dd:ee:ff");
            var upper = new MacAddress("AA:BB:CC:DD:EE:FF");

            Assert.Equal(lower, upper);
            Assert.Equal(lower.GetHashCode(), upper.GetHashCode());
        }

        [Theory]
        [InlineData(-23.5505, -46.6333)]
        [InlineData(90.0, 180.0)]
        [InlineData(-90.0, -180.0)]
        public void GeoPosition_ValidCoordinates_CreatesPosition(double latitude, double longitude)
        {
            var position = new GeoPosition(latitude, longitude);

            Assert.Equal(latitude, position.Latitude);
            Assert.Equal(longitude, position.Longitude);
        }

        [Theory]
        [InlineData(-90.1, -46.6333)]
        [InlineData(90.1, -46.6333)]
        [InlineData(-23.5505, -180.1)]
        [InlineData(-23.5505, 180.1)]
        public void GeoPosition_InvalidCoordinates_Throws(double latitude, double longitude)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new GeoPosition(latitude, longitude));
        }

        [Fact]
        public void BeaconReading_ZeroTxPowerInConvenienceConstructor_IsTreatedAsUnknown()
        {
            var reading = new BeaconReading("AA:BB:CC:DD:EE:01", rssi: -65, txPower: 0);

            Assert.Equal(new MacAddress("AA:BB:CC:DD:EE:01"), reading.Mac);
            Assert.Equal(-65, reading.Rssi);
            Assert.Null(reading.TxPower);
        }

        [Fact]
        public void BeaconReading_InvalidRssi_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new BeaconReading("AA:BB:CC:DD:EE:01", rssi: 0, txPower: -59));
        }

        [Fact]
        public void BeaconSample_From_UsesInjectedDistanceCalculator()
        {
            var reading = new BeaconReading("AA:BB:CC:DD:EE:01", rssi: -65, txPower: -59);
            var anchor = new BeaconAnchor("AA:BB:CC:DD:EE:01", -23.5505, -46.6333);
            var calculator = new FixedDistanceCalculator(distanceMeters: 7.5);

            var sample = BeaconSample.From(reading, anchor, calculator);

            Assert.Equal(reading.Mac, sample.Mac);
            Assert.Equal(anchor.Position, sample.Position);
            Assert.Equal(reading.Rssi, sample.Rssi);
            Assert.Equal(reading.TxPower, sample.TxPower);
            Assert.Equal(7.5, sample.EstimatedDistanceMeters);
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
    }
}
