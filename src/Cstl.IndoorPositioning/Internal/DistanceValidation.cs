namespace Cstl.IndoorPositioning.Internal
{
    internal static class DistanceValidation
    {
        public static void ThrowIfNotPositiveOrInvalid(double value, string parameterName)
        {
            if (value <= 0 || double.IsNaN(value) || double.IsInfinity(value))
                throw new ArgumentOutOfRangeException(parameterName, "Distance must be positive and finite.");
        }
    }
}
