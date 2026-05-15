using Cstl.IndoorPositioning.Abstractions.Enums;
using Cstl.IndoorPositioning.Abstractions.Extensions;

namespace Cstl.IndoorPositioning.Abstractions.Configuration
{
    /// <summary>
    /// General configuration for the indoor positioning pipeline.
    /// </summary>
    public sealed class IndoorPositioningOptions
    {
        /// <summary>
        /// Environment profile used when <see cref="PathLossExponent" /> is not provided.
        /// </summary>
        public EnvironmentProfile EnvironmentProfile { get; set; } = EnvironmentProfile.Indoor;

        /// <summary>
        /// Custom path-loss exponent. When null, the value comes from <see cref="EnvironmentProfile" />.
        /// </summary>
        public double? PathLossExponent { get; set; }

        /// <summary>
        /// Default calibrated TxPower at one meter, in dBm.
        /// </summary>
        public int DefaultTxPower { get; set; } = -59;

        /// <summary>
        /// Enables stateful Kalman smoothing in the default locator.
        /// </summary>
        public bool EnableSmoothing { get; set; } = true;

        /// <summary>
        /// Kalman smoothing settings.
        /// </summary>
        public KalmanSmoothingOptions Smoothing { get; set; } = new KalmanSmoothingOptions();

        /// <summary>
        /// Gets the final path-loss exponent that should be used by the distance calculator.
        /// </summary>
        public double ResolvePathLossExponent()
        {
            return PathLossExponent ?? EnvironmentProfile.ToPathLossExponent();
        }

        /// <summary>
        /// Validates the current options.
        /// </summary>
        public void Validate()
        {
            var pathLossExponent = ResolvePathLossExponent();

            if (pathLossExponent <= 0 || double.IsNaN(pathLossExponent) || double.IsInfinity(pathLossExponent))
                throw new ArgumentOutOfRangeException(nameof(PathLossExponent), "Path-loss exponent must be positive and finite.");

            if (DefaultTxPower == 0)
                throw new ArgumentOutOfRangeException(nameof(DefaultTxPower), "Default TxPower cannot be zero. Use null TxPower in readings when unknown.");

            Smoothing ??= new KalmanSmoothingOptions();
            Smoothing.Validate();
        }
    }
}
