using Cstl.IndoorPositioning.Abstractions.Models;
using Cstl.IndoorPositioning.Abstractions.Services;
using System.Collections.Generic;

namespace Cstl.IndoorPositioning
{
    public static class TrilaterationEngine
    {
        private static readonly ITrilaterationEngine DefaultEngine = TrilaterationEngineFactory.CreateDefault();

        public static TrilaterationResult Estimate(IReadOnlyList<BeaconSample> beacons)
        {
            return DefaultEngine.Estimate(beacons);
        }
    }
}
