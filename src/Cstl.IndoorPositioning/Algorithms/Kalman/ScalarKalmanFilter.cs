using Cstl.IndoorPositioning.Abstractions.Configuration;

namespace Cstl.IndoorPositioning.Algorithms.Kalman
{
    internal sealed class ScalarKalmanFilter
    {
        private readonly double _processNoise;
        private readonly double _measurementNoise;
        private readonly double _initialErrorCovariance;

        private double _estimate;
        private double _errorCovariance;

        public ScalarKalmanFilter(KalmanSmoothingOptions options)
        {
            _processNoise = options.ProcessNoise;
            _measurementNoise = options.MeasurementNoise;
            _initialErrorCovariance = options.InitialErrorCovariance;
            _errorCovariance = _initialErrorCovariance;
        }

        public void Initialize(double initialValue)
        {
            _estimate = initialValue;
            _errorCovariance = _initialErrorCovariance;
        }

        public double Update(double measurement)
        {
            var predictedErrorCovariance = _errorCovariance + _processNoise;
            var kalmanGain = predictedErrorCovariance / (predictedErrorCovariance + _measurementNoise);

            _estimate += kalmanGain * (measurement - _estimate);
            _errorCovariance = (1.0 - kalmanGain) * predictedErrorCovariance;

            return _estimate;
        }

        public void Reset()
        {
            _estimate = 0;
            _errorCovariance = _initialErrorCovariance;
        }
    }
}
