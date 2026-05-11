using Cstl.IndoorPositioning.Abstractions.Models;
using System;

namespace Cstl.IndoorPositioning.Geometry
{
    internal sealed class EquirectangularGeoProjection : IGeoProjection
    {
        private const double EarthRadiusMeters = 6_371_000.0;
        private const double DegreesToRadians = Math.PI / 180.0;
        private const double RadiansToDegrees = 180.0 / Math.PI;

        public LocalCoordinate ToLocalCoordinate(BeaconSample beacon, GeoPoint origin)
        {
            var x = ProjectLongitudeToMeters(
                longitude: beacon.Longitude,
                longitudeReference: origin.Longitude,
                latitudeReference: origin.Latitude);

            var y = ProjectLatitudeToMeters(
                latitude: beacon.Latitude,
                latitudeReference: origin.Latitude);

            return new LocalCoordinate(x, y);
        }

        public GeoPoint ToGeoPoint(LocalCoordinate coordinate, GeoPoint origin)
        {
            var latitude = origin.Latitude
                + (coordinate.Y / EarthRadiusMeters)
                * RadiansToDegrees;

            var longitude = origin.Longitude
                + (coordinate.X / (EarthRadiusMeters * Math.Cos(origin.Latitude * DegreesToRadians)))
                * RadiansToDegrees;

            return new GeoPoint(latitude, longitude);
        }

        private static double ProjectLatitudeToMeters(double latitude, double latitudeReference)
        {
            return (latitude - latitudeReference) * DegreesToRadians * EarthRadiusMeters;
        }

        private static double ProjectLongitudeToMeters(double longitude, double longitudeReference, double latitudeReference)
        {
            return (longitude - longitudeReference)
                * DegreesToRadians
                * EarthRadiusMeters
                * Math.Cos(latitudeReference * DegreesToRadians);
        }
    }
}
