namespace Cstl.IndoorPositioning.Abstractions.Configuration
{
    /// <summary>
    /// Configuration for scalar Kalman smoothing over latitude and longitude.
    /// </summary>
    public sealed class KalmanSmoothingOptions
    {
        /// <summary>Process noise. Lower values assume slower movement.</summary>
        public double ProcessNoise { get; set; } = 1e-5;

        /// <summary>Measurement noise. Higher values produce stronger smoothing.</summary>
        public double MeasurementNoise { get; set; } = 1e-3;

        /// <summary>Initial uncertainty covariance.</summary>
        public double InitialErrorCovariance { get; set; } = 1.0;

        /// <summary>
        /// Validates the current options.
        /// </summary>
        public void Validate()
        {
            if (ProcessNoise <= 0 || double.IsNaN(ProcessNoise) || double.IsInfinity(ProcessNoise))
                throw new ArgumentOutOfRangeException(nameof(ProcessNoise), "Process noise must be positive and finite.");

            if (MeasurementNoise <= 0 || double.IsNaN(MeasurementNoise) || double.IsInfinity(MeasurementNoise))
                throw new ArgumentOutOfRangeException(nameof(MeasurementNoise), "Measurement noise must be positive and finite.");

            if (InitialErrorCovariance <= 0 || double.IsNaN(InitialErrorCovariance) || double.IsInfinity(InitialErrorCovariance))
                throw new ArgumentOutOfRangeException(nameof(InitialErrorCovariance), "Initial error covariance must be positive and finite.");
        }
    }
}
