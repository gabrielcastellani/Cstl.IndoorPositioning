using Cstl.IndoorPositioning.Abstractions.Models;
using System.Collections.Generic;

namespace Cstl.IndoorPositioning.Abstractions.Services
{
    public interface ITrilaterationEngine
    {
        TrilaterationResult Estimate(IReadOnlyList<BeaconSample> beacons);
    }
}
