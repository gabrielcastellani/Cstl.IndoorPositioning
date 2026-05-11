using Cstl.IndoorPositioning.Geometry;
using System;

namespace Cstl.IndoorPositioning.LinearAlgebra
{
    internal static class LeastSquaresLinearSystemBuilder
    {
        public static LinearSystem Build(LocalPoint[] points)
        {
            var referencePoint = points[points.Length - 1];
            var rows = points.Length - 1;

            var matrixA = new double[rows, 2];
            var vectorB = new double[rows];
            var weights = new double[rows];

            for (var i = 0; i < rows; i++)
            {
                var point = points[i];

                matrixA[i, 0] = 2.0 * (point.X - referencePoint.X);
                matrixA[i, 1] = 2.0 * (point.Y - referencePoint.Y);

                vectorB[i] =
                    SquaredDifference(point.X, referencePoint.X) +
                    SquaredDifference(point.Y, referencePoint.Y) -
                    SquaredDifference(point.Distance, referencePoint.Distance);

                weights[i] = Math.Sqrt(point.Weight * referencePoint.Weight);
            }

            return new LinearSystem(matrixA, vectorB, weights);
        }

        private static double SquaredDifference(double first, double second)
        {
            return (first * first) - (second * second);
        }
    }
}
