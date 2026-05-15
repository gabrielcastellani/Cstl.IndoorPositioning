namespace Cstl.IndoorPositioning.Abstractions.Enums
{
    /// <summary>
    /// Physical deployment environment profile.
    /// </summary>
    public enum EnvironmentProfile
    {
        /// <summary>Open free-space environment. N ≈ 2.0.</summary>
        FreeSpace = 0,

        /// <summary>Open hall or office without many divisions. N ≈ 2.5.</summary>
        OpenSpace = 1,

        /// <summary>Typical indoor office with walls. N ≈ 3.0.</summary>
        Indoor = 2,

        /// <summary>Dense corridors or multiple walls. N ≈ 3.5.</summary>
        Obstructed = 3,

        /// <summary>Industrial environment with high metal density. N ≈ 4.0.</summary>
        Industrial = 4
    }
}
