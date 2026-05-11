using Cstl.IndoorPositioning.Geometry;

namespace Cstl.IndoorPositioning.LinearAlgebra
{
    internal interface ILinearSystemSolver
    {
        bool TrySolve(LinearSystem system, out LocalCoordinate coordinate);
    }
}
