using Cstl.IndoorPositioning.Abstractions.Configuration;
using Cstl.IndoorPositioning.Abstractions.Interfaces;
using Cstl.IndoorPositioning.Abstractions.Models;
using Cstl.IndoorPositioning.Algorithms.Kalman;

namespace Cstl.IndoorPositioning.Algorithms
{
    /// <summary>
    /// Stateful scalar Kalman smoother applied independently to latitude and longitude.
    /// </summary>
    public sealed class KalmanPositionSmoother : IPositionSmoother
    {
        private readonly object _syncRoot = new object();
        private readonly ScalarKalmanFilter _latitudeFilter;
        private readonly ScalarKalmanFilter _longitudeFilter;
        private bool _initialized;

        /// <summary>
        /// Creates a Kalman smoother with default options.
        /// </summary>
        public KalmanPositionSmoother()
            : this(new KalmanSmoothingOptions())
        { }

        /// <summary>
        /// Creates a Kalman smoother with custom options.
        /// </summary>
        public KalmanPositionSmoother(KalmanSmoothingOptions options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));

            options.Validate();

            _latitudeFilter = new ScalarKalmanFilter(options);
            _longitudeFilter = new ScalarKalmanFilter(options);
        }

        /// <inheritdoc />
        public TrilaterationResult Smooth(TrilaterationResult raw)
        {
            if (raw is null)
                throw new ArgumentNullException(nameof(raw));

            lock (_syncRoot)
            {
                if (!_initialized)
                {
                    _latitudeFilter.Initialize(raw.Latitude);
                    _longitudeFilter.Initialize(raw.Longitude);
                    _initialized = true;
                    return raw;
                }

                var latitude = _latitudeFilter.Update(raw.Latitude);
                var longitude = _longitudeFilter.Update(raw.Longitude);

                return new TrilaterationResult(
                    new GeoPosition(latitude, longitude),
                    raw.BeaconsUsed,
                    raw.AccuracyMeters,
                    raw.Method);
            }
        }

        /// <inheritdoc />
        public void Reset()
        {
            lock (_syncRoot)
            {
                _latitudeFilter.Reset();
                _longitudeFilter.Reset();
                _initialized = false;
            }
        }
    }
}
