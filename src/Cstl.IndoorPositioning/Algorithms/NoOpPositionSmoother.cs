using Cstl.IndoorPositioning.Abstractions.Interfaces;
using Cstl.IndoorPositioning.Abstractions.Models;

namespace Cstl.IndoorPositioning.Algorithms
{
    public sealed class NoOpPositionSmoother : IPositionSmoother
    {
        public TrilaterationResult Smooth(TrilaterationResult raw)
        {
            if (raw is null)
                throw new ArgumentNullException(nameof(raw));

            return raw;
        }

        public void Reset() { }
    }
}
