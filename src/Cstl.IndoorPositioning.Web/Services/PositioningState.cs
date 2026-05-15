using Cstl.IndoorPositioning.Abstractions.Models;
using Cstl.IndoorPositioning.Web.Model;

namespace Cstl.IndoorPositioning.Web.Services
{
    public class PositioningState
    {
        public List<BeaconViewModel> Beacons { get; } = new();
        public TrilaterationResult? LastResult { get; private set; }
        public event Action? OnChanged;

        public double MapCenterLat { get; set; } = -23.5505;
        public double MapCenterLon { get; set; } = -46.6333;
        public int MapZoom { get; set; } = 17;

        public bool LiveMode { get; set; } = false;


        public void AddBeacon(BeaconViewModel beacon)
        {
            Beacons.Add(beacon);
            NotifyChanged();
        }

        public void RemoveBeacon(string mac)
        {
            Beacons.RemoveAll(b => b.MAC.Equals(mac, StringComparison.OrdinalIgnoreCase));
            NotifyChanged();
        }

        public void UpdateBeaconPosition(string mac, double lat, double lon)
        {
            var beacon = FindByMac(mac);
            if (beacon is null) return;
            beacon.Latitude = lat;
            beacon.Longitude = lon;
            NotifyChanged();
        }

        public void UpdateBeaconRssi(string mac, int rssi)
        {
            var beacon = FindByMac(mac);
            if (beacon is null) return;
            beacon.RSSI = rssi;
            NotifyChanged();
        }

        public void ToggleBeaconActive(string mac)
        {
            var beacon = FindByMac(mac);
            if (beacon is null) return;
            beacon.Active = !beacon.Active;
            NotifyChanged();
        }

        public TrilaterationResult? Recalculate()
        {
            var active = Beacons.Where(b => b.Active).ToList();

            if (active.Count == 0)
            {
                LastResult = null;
                NotifyChanged();

                return null;
            }

            // Use the convenience string constructor: (mac, lat, lon, rssi, txPower, distMeters)
            var samples = active.Select(b => new BeaconSample(
                b.MAC,
                b.Latitude,
                b.Longitude,
                b.RSSI,
                b.TxPower,
                b.EstimatedDistanceMeters
            )).ToArray();

            LastResult = TrilaterationEngine.Estimate(samples);
            NotifyChanged();
            return LastResult;
        }

        public TrilaterationResult? RecalculateFromReadings(IEnumerable<BeaconReading> readings)
        {
            var anchors = Beacons.ToDictionary(b => b.MAC.ToUpperInvariant());

            var samples = readings
                .Where(r => anchors.ContainsKey(r.Mac.Value.ToUpperInvariant()))
                .Select(r =>
                {
                    var anchor = anchors[r.Mac.Value.ToUpperInvariant()];
                    return new BeaconSample(
                        r.Mac.Value,
                        anchor.Latitude,
                        anchor.Longitude,
                        r.Rssi,
                        r.TxPower ?? 0,
                        BeaconDistanceCalculator.Calculate(r.Rssi, r.TxPower)
                    );
                })
                .ToArray();

            if (samples.Length == 0) { LastResult = null; NotifyChanged(); return null; }

            LastResult = TrilaterationEngine.Estimate(samples);
            NotifyChanged();
            return LastResult;
        }


        private BeaconViewModel? FindByMac(string mac)
        {
            return Beacons.FirstOrDefault(b => b.MAC.Equals(mac, StringComparison.OrdinalIgnoreCase));
        }

        private void NotifyChanged() => OnChanged?.Invoke();
    }
}
