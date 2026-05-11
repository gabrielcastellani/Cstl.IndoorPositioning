using Cstl.IndoorPositioning.Abstractions.Enums;
using Cstl.IndoorPositioning.Abstractions.Models;

namespace Cstl.IndoorPositioning.Tests
{
    public class BeaconLocatorTests
    {
        private static readonly BeaconAnchor[] Anchors =
        [
            new() { MAC = "AA:BB:CC:DD:EE:01", Latitude = -23.5505, Longitude = -46.6333 },
            new() { MAC = "AA:BB:CC:DD:EE:02", Latitude = -23.5505, Longitude = -46.6320 },
            new() { MAC = "AA:BB:CC:DD:EE:03", Latitude = -23.5495, Longitude = -46.6326 },
            new() { MAC = "AA:BB:CC:DD:EE:04", Latitude = -23.5515, Longitude = -46.6326 },
        ];

        private static int DistToRssi(double distMeters, int txPower = -59, double n = 3.0)
        {
            return (int)Math.Round(BeaconDistanceCalculator.DistanceToRssi(distMeters, txPower, n));
        }

        [Fact]
        public void Locate_ThreeBeacons_EstimatesPositionWithinTolerance()
        {
            double targetLat = -23.5505, targetLon = -46.6326;

            var readings = new[]
            {
                new BeaconReading { MAC = "AA:BB:CC:DD:EE:01", TxPower = -59,
                    RSSI = DistToRssi(HaversineMeters(-23.5505, -46.6333, targetLat, targetLon)) },
                new BeaconReading { MAC = "AA:BB:CC:DD:EE:02", TxPower = -59,
                    RSSI = DistToRssi(HaversineMeters(-23.5505, -46.6320, targetLat, targetLon)) },
                new BeaconReading { MAC = "AA:BB:CC:DD:EE:03", TxPower = -59,
                    RSSI = DistToRssi(HaversineMeters(-23.5495, -46.6326, targetLat, targetLon)) },
            };

            var locator = new BeaconLocator();
            var result = locator.Locate(readings, Anchors);

            Assert.Equal(EstimationMethod.Trilateration, result.Method);
            Assert.Equal(3, result.BeaconsUsed);

            var errorMeters = HaversineMeters(result.Latitude, result.Longitude, targetLat, targetLon);

            Assert.True(errorMeters < 10.0, $"Error of {errorMeters:F2}m — RSSI rounding introduces inaccuracy.");
        }

        [Fact]
        public void Locate_UnknownMac_IsIgnored()
        {
            var readings = new[]
            {
                new BeaconReading { MAC = "AA:BB:CC:DD:EE:01", TxPower = -59, RSSI = -65 },
                new BeaconReading { MAC = "FF:FF:FF:FF:FF:FF", TxPower = -59, RSSI = -50 },
            };

            var locator = new BeaconLocator();
            var result = locator.Locate(readings, Anchors);

            Assert.Equal(EstimationMethod.Proximity, result.Method);
            Assert.Equal(1, result.BeaconsUsed);
        }

        [Fact]
        public void Locate_AllMacsUnknown_Throws()
        {
            var readings = new[]
            {
            new BeaconReading { MAC = "FF:FF:FF:FF:FF:FF", TxPower = -59, RSSI = -50 }
        };

            var locator = new BeaconLocator();
            Assert.Throws<ArgumentException>(() => locator.Locate(readings, Anchors));
        }

        [Fact]
        public void Locate_MacIsCaseInsensitive()
        {
            var readings = new[]
            {
                new BeaconReading { MAC = "aa:bb:cc:dd:ee:01", TxPower = -59, RSSI = -65 }
            };

            var locator = new BeaconLocator();
            var result = locator.Locate(readings, Anchors);
            Assert.Equal(1, result.BeaconsUsed);
        }

        [Fact]
        public void Locate_FourBeacons_ReturnsResult()
        {
            double targetLat = -23.5505, targetLon = -46.6326;

            var readings = Anchors.Select(a => new BeaconReading
            {
                MAC = a.MAC,
                TxPower = -59,
                RSSI = DistToRssi(HaversineMeters(a.Latitude, a.Longitude, targetLat, targetLon))
            }).ToArray();

            var locator = new BeaconLocator();
            var result = locator.Locate(readings, Anchors);

            Assert.Equal(4, result.BeaconsUsed);
            Assert.Equal(EstimationMethod.Trilateration, result.Method);
        }

        private static double HaversineMeters(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6_371_000.0;

            double dLat = (lat2 - lat1) * Math.PI / 180.0;
            double dLon = (lon2 - lon1) * Math.PI / 180.0;
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                     + Math.Cos(lat1 * Math.PI / 180.0) * Math.Cos(lat2 * Math.PI / 180.0)
                     * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        }
    }
}
