using Cstl.IndoorPositioning.Abstractions.Enums;

namespace Cstl.IndoorPositioning.Abstractions.Extensions
{
    /// <summary>
    /// Extensions for <see cref="EnvironmentProfile" />.
    /// </summary>
    public static class EnvironmentProfileExtensions
    {
        /// <summary>
        /// Returns the suggested Log-Distance Path Loss exponent for the environment.
        /// </summary>
        public static double ToPathLossExponent(this EnvironmentProfile profile)
        {
            return profile switch
            {
                EnvironmentProfile.FreeSpace => 2.0,
                EnvironmentProfile.OpenSpace => 2.5,
                EnvironmentProfile.Indoor => 3.0,
                EnvironmentProfile.Obstructed => 3.5,
                EnvironmentProfile.Industrial => 4.0,
                _ => 3.0
            };
        }
    }
}
