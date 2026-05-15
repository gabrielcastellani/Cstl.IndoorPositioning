namespace Cstl.IndoorPositioning.Abstractions.Enums
{
    /// <summary>
    /// Method used to estimate the device position.
    /// </summary>
    public enum EstimationMethod
    {
        /// <summary>Three or more beacons using Weighted Least Squares.</summary>
        Trilateration = 0,

        /// <summary>Two beacons using a weighted midpoint.</summary>
        Bilateration = 1,

        /// <summary>One beacon using proximity to the anchor position.</summary>
        Proximity = 2,

        /// <summary>Fallback weighted centroid when the geometric solution is unstable.</summary>
        WeightedCentroid = 3
    }
}
