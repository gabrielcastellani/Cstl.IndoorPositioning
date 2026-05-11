using Cstl.IndoorPositioning.Geometry;
using System;

namespace Cstl.IndoorPositioning.LinearAlgebra
{
    internal sealed class WeightedNormalEquationSolver : ILinearSystemSolver
    {
        private const double DeterminantThreshold = 1e-10;

        public bool TrySolve(LinearSystem system, out LocalCoordinate coordinate)
        {
            var equations = BuildNormalEquations(system);

            if (Math.Abs(equations.Determinant) < DeterminantThreshold)
            {
                coordinate = default(LocalCoordinate);
                return false;
            }

            var x =
                ((equations.B1 * equations.A22) - (equations.B2 * equations.A12)) /
                equations.Determinant;

            var y =
                ((equations.A11 * equations.B2) - (equations.A12 * equations.B1)) /
                equations.Determinant;

            coordinate = new LocalCoordinate(x, y);
            return true;
        }

        private static NormalEquations BuildNormalEquations(LinearSystem system)
        {
            double a11 = 0;
            double a12 = 0;
            double a22 = 0;
            double b1 = 0;
            double b2 = 0;

            for (var i = 0; i < system.Weights.Length; i++)
            {
                var weight = system.Weights[i];

                var ax = system.MatrixA[i, 0];
                var ay = system.MatrixA[i, 1];
                var b = system.VectorB[i];

                a11 += weight * ax * ax;
                a12 += weight * ax * ay;
                a22 += weight * ay * ay;

                b1 += weight * ax * b;
                b2 += weight * ay * b;
            }

            var determinant = (a11 * a22) - (a12 * a12);

            return new NormalEquations(a11, a12, a22, b1, b2, determinant);
        }
    }
}
