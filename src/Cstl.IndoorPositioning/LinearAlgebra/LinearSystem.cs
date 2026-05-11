namespace Cstl.IndoorPositioning.LinearAlgebra
{
    internal sealed class LinearSystem
    {
        public double[,] MatrixA { get; }
        public double[] VectorB { get; }
        public double[] Weights { get; }

        public LinearSystem(double[,] matrixA, double[] vectorB, double[] weights)
        {
            MatrixA = matrixA;
            VectorB = vectorB;
            Weights = weights;
        }
    }
}
