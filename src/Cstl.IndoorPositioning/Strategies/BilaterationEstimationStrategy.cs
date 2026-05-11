using Cstl.IndoorPositioning.Abstractions.Enums;
using Cstl.IndoorPositioning.Abstractions.Models;
using Cstl.IndoorPositioning.Weighting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cstl.IndoorPositioning.Strategies
{
    internal sealed class BilaterationEstimationStrategy : IPositionEstimationStrategy
    {
        private readonly IBeaconWeightCalculator _weightCalculator;

        public BilaterationEstimationStrategy(IBeaconWeightCalculator weightCalculator)
        {
            _weightCalculator = weightCalculator;
        }

        public bool CanEstimate(int beaconCount)
        {
            return beaconCount == 2;
        }

        public TrilaterationResult Estimate(IReadOnlyList<BeaconSample> beacons)
        {
            var first = beacons[0];
            var second = beacons[1];

            var firstWeight = _weightCalculator.Calculate(first.EstimatedDistanceMeters);
            var secondWeight = _weightCalculator.Calculate(second.EstimatedDistanceMeters);
            var totalWeight = firstWeight + secondWeight;

            return new TrilaterationResult
            {
                Latitude = WeightedAverage(
                    first.Latitude,
                    firstWeight,
                    second.Latitude,
                    secondWeight,
                    totalWeight),

                Longitude = WeightedAverage(
                    first.Longitude,
                    firstWeight,
                    second.Longitude,
                    secondWeight,
                    totalWeight),

                BeaconsUsed = 2,
                AccuracyMeters = (first.EstimatedDistanceMeters + second.EstimatedDistanceMeters) / 2.0,
                Method = EstimationMethod.Bilateration
            };
        }

        private static double WeightedAverage(
            double firstValue,
            double firstWeight,
            double secondValue,
            double secondWeight,
            double totalWeight)
        {
            return ((firstValue * firstWeight) + (secondValue * secondWeight)) / totalWeight;
        }
    }
}
