using Cstl.IndoorPositioning.Abstractions.Models;

namespace Cstl.IndoorPositioning.Geometry
{
    internal interface IGeoProjection
    {
        LocalCoordinate ToLocalCoordinate(BeaconSample beacon, GeoPoint origin);
        GeoPoint ToGeoPoint(LocalCoordinate coordinate, GeoPoint origin);
    }
}
