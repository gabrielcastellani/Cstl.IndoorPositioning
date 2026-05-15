using Cstl.IndoorPositioning.Abstractions.Enums;
using Cstl.IndoorPositioning.Abstractions.Interfaces;
using Cstl.IndoorPositioning.Abstractions.Models;
using Cstl.IndoorPositioning.Algorithms.Squares;

namespace Cstl.IndoorPositioning.Algorithms
{
    /// <summary>
    /// Estimates position using Weighted Least Squares with RSSI-based weights.
    /// </summary>
    public sealed class WeightedLeastSquaresPositionEstimator : IPositionEstimator
    {
        private const double EarthRadiusMeters = 6_371_000.0;
        private const double DegreesToRadians = Math.PI / 180.0;
        private const double RadiansToDegrees = 180.0 / Math.PI;
        private const double RelativeCollinearityThreshold = 1e-10;

        /// <inheritdoc />
        public TrilaterationResult Estimate(IReadOnlyList<BeaconSample> samples)
        {
            GuardNotEmpty(samples);

            return samples.Count switch
            {
                1 => EstimateProximity(samples[0]),
                2 => EstimateBilateration(samples),
                _ => EstimateWls(samples)
            };
        }

        private static TrilaterationResult EstimateProximity(BeaconSample sample)
        {
            return new TrilaterationResult(
                sample.Position,
                beaconsUsed: 1,
                accuracyMeters: sample.EstimatedDistanceMeters,
                method: EstimationMethod.Proximity);
        }

        private static TrilaterationResult EstimateBilateration(IReadOnlyList<BeaconSample> samples)
        {
            var first = samples[0];
            var second = samples[1];
            var firstWeight = RssiWeight(first.Rssi);
            var secondWeight = RssiWeight(second.Rssi);
            var totalWeight = firstWeight + secondWeight;

            return new TrilaterationResult(
                latitude: ((first.Latitude * firstWeight) + (second.Latitude * secondWeight)) / totalWeight,
                longitude: ((first.Longitude * firstWeight) + (second.Longitude * secondWeight)) / totalWeight,
                beaconsUsed: 2,
                accuracyMeters: (first.EstimatedDistanceMeters + second.EstimatedDistanceMeters) / 2.0,
                method: EstimationMethod.Bilateration);
        }

        private static TrilaterationResult EstimateWls(IReadOnlyList<BeaconSample> samples)
        {
            var origin = samples[0].Position;
            var points = Project(samples, origin);
            var equations = BuildNormalEquations(points);

            if (IsCollinear(equations))
                return WeightedCentroid(samples);

            var solution = SolveCramer(equations);

            if (IsSolutionImplausible(points, solution.X, solution.Y))
                return WeightedCentroid(samples);

            return new TrilaterationResult(
                latitude: UnprojectY(solution.Y, origin.Latitude),
                longitude: UnprojectX(solution.X, origin.Longitude, origin.Latitude),
                beaconsUsed: samples.Count,
                accuracyMeters: WeightedResidual(points, solution.X, solution.Y),
                method: EstimationMethod.Trilateration);
        }

        private static LocalPoint[] Project(IReadOnlyList<BeaconSample> samples, GeoPosition origin)
        {
            var points = new LocalPoint[samples.Count];

            for (var i = 0; i < samples.Count; i++)
            {
                var sample = samples[i];
                points[i] = new LocalPoint(
                    x: ProjectLon(sample.Longitude, origin.Longitude, origin.Latitude),
                    y: ProjectLat(sample.Latitude, origin.Latitude),
                    distanceMeters: sample.EstimatedDistanceMeters,
                    weight: RssiWeight(sample.Rssi));
            }

            return points;
        }

        private static NormalEquations BuildNormalEquations(LocalPoint[] points)
        {
            var reference = points[points.Length - 1];
            double a11 = 0;
            double a12 = 0;
            double a22 = 0;
            double b1 = 0;
            double b2 = 0;

            for (var i = 0; i < points.Length - 1; i++)
            {
                var point = points[i];
                var ai0 = 2.0 * (point.X - reference.X);
                var ai1 = 2.0 * (point.Y - reference.Y);
                var bi =
                    (point.X * point.X - reference.X * reference.X) +
                    (point.Y * point.Y - reference.Y * reference.Y) -
                    (point.DistanceMeters * point.DistanceMeters - reference.DistanceMeters * reference.DistanceMeters);

                var weight = Math.Sqrt(point.Weight * reference.Weight);

                a11 += weight * ai0 * ai0;
                a12 += weight * ai0 * ai1;
                a22 += weight * ai1 * ai1;
                b1 += weight * ai0 * bi;
                b2 += weight * ai1 * bi;
            }

            return new NormalEquations(a11, a12, a22, b1, b2);
        }

        private static Solution SolveCramer(NormalEquations equations)
        {
            return new Solution(
                x: (equations.B1 * equations.A22 - equations.B2 * equations.A12) / equations.Determinant,
                y: (equations.A11 * equations.B2 - equations.A12 * equations.B1) / equations.Determinant);
        }

        private static TrilaterationResult WeightedCentroid(IReadOnlyList<BeaconSample> samples)
        {
            var totalWeight = 0.0;
            var latitudeSum = 0.0;
            var longitudeSum = 0.0;
            var accuracySum = 0.0;

            foreach (var sample in samples)
            {
                var weight = RssiWeight(sample.Rssi);
                latitudeSum += sample.Latitude * weight;
                longitudeSum += sample.Longitude * weight;
                totalWeight += weight;
                accuracySum += sample.EstimatedDistanceMeters;
            }

            return new TrilaterationResult(
                latitude: latitudeSum / totalWeight,
                longitude: longitudeSum / totalWeight,
                beaconsUsed: samples.Count,
                accuracyMeters: accuracySum / samples.Count,
                method: EstimationMethod.WeightedCentroid);
        }

        private static double WeightedResidual(LocalPoint[] points, double estimatedX, double estimatedY)
        {
            var totalWeight = 0.0;
            var weightedError = 0.0;

            foreach (var point in points)
            {
                var distance = Math.Sqrt(
                    (estimatedX - point.X) * (estimatedX - point.X) +
                    (estimatedY - point.Y) * (estimatedY - point.Y));

                weightedError += point.Weight * Math.Abs(distance - point.DistanceMeters);
                totalWeight += point.Weight;
            }

            return totalWeight > 0 ? weightedError / totalWeight : double.NaN;
        }

        private static double ProjectLat(double latitude, double latitudeReference)
            => (latitude - latitudeReference) * DegreesToRadians * EarthRadiusMeters;

        private static double ProjectLon(double longitude, double longitudeReference, double latitudeReference)
            => (longitude - longitudeReference) * DegreesToRadians * EarthRadiusMeters * Math.Cos(latitudeReference * DegreesToRadians);

        private static double UnprojectY(double y, double latitudeReference)
            => latitudeReference + (y / EarthRadiusMeters) * RadiansToDegrees;

        private static double UnprojectX(double x, double longitudeReference, double latitudeReference)
            => longitudeReference + (x / (EarthRadiusMeters * Math.Cos(latitudeReference * DegreesToRadians))) * RadiansToDegrees;

        private static double RssiWeight(int rssi) => Math.Pow(10.0, rssi / 10.0);

        private static bool IsSolutionImplausible(LocalPoint[] points, double estimatedX, double estimatedY)
        {
            foreach (var point in points)
            {
                var distance = Math.Sqrt(
                    (estimatedX - point.X) * (estimatedX - point.X) +
                    (estimatedY - point.Y) * (estimatedY - point.Y));

                if (distance <= point.DistanceMeters * 3.0 + 1.0)
                    return false;
            }

            return true;
        }

        private static bool IsCollinear(NormalEquations equations)
        {
            var scale = Math.Abs(equations.A11) * Math.Abs(equations.A22);
            if (scale < double.Epsilon)
                return true;

            return Math.Abs(equations.Determinant) / scale < RelativeCollinearityThreshold;
        }

        private static void GuardNotEmpty(IReadOnlyList<BeaconSample> samples)
        {
            if (samples is null || samples.Count == 0)
                throw new ArgumentException("At least one beacon sample is required.", nameof(samples));
        }
    }
}
