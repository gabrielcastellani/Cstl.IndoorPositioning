namespace Cstl.IndoorPositioning.Weighting
{
    internal interface IBeaconWeightCalculator
    {
        double Calculate(double distanceMeters);
    }
}
