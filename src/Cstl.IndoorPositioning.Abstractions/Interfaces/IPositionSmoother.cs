using Cstl.IndoorPositioning.Abstractions.Models;

namespace Cstl.IndoorPositioning.Abstractions.Interfaces
{
    /// <summary>
    /// Smooths consecutive position estimates.
    /// </summary>
    public interface IPositionSmoother
    {
        /// <summary>
        /// Applies smoothing to a raw position estimate.
        /// </summary>
        TrilaterationResult Smooth(TrilaterationResult raw);

        /// <summary>
        /// Clears the internal smoothing state.
        /// </summary>
        void Reset();
    }
}
