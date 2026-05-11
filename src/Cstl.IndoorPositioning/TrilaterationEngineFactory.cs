using Cstl.IndoorPositioning.Abstractions.Services;
using Cstl.IndoorPositioning.Accuracy;
using Cstl.IndoorPositioning.Fallback;
using Cstl.IndoorPositioning.Geometry;
using Cstl.IndoorPositioning.LinearAlgebra;
using Cstl.IndoorPositioning.Strategies;
using Cstl.IndoorPositioning.Weighting;

namespace Cstl.IndoorPositioning
{
    public static class TrilaterationEngineFactory
    {
        public static ITrilaterationEngine CreateDefault()
        {
            var weightCalculator = new InverseSquareDistanceWeightCalculator();
            var projection = new EquirectangularGeoProjection();
            var solver = new WeightedNormalEquationSolver();
            var accuracyCalculator = new WeightedResidualAccuracyCalculator();
            var fallbackEstimator = new WeightedCentroidFallbackEstimator(weightCalculator);

            var strategies = new IPositionEstimationStrategy[]
            {
                new ProximityEstimationStrategy(),
                new BilaterationEstimationStrategy(weightCalculator),
                new WeightedLeastSquaresEstimationStrategy(
                    projection,
                    weightCalculator,
                    solver,
                    fallbackEstimator,
                    accuracyCalculator)
            };

            return new DefaultTrilaterationEngine(strategies);
        }
    }
}
