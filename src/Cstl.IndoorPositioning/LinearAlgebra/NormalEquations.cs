using System;
using System.Collections.Generic;
using System.Text;

namespace Cstl.IndoorPositioning.LinearAlgebra
{
    internal sealed class NormalEquations
    {
        public double A11 { get; }
        public double A12 { get; }
        public double A22 { get; }
        public double B1 { get; }
        public double B2 { get; }
        public double Determinant { get; }

        public NormalEquations(double a11, double a12, double a22, double b1, double b2, double determinant)
        {
            A11 = a11;
            A12 = a12;
            A22 = a22;
            B1 = b1;
            B2 = b2;
            Determinant = determinant;
        }
    }
}
