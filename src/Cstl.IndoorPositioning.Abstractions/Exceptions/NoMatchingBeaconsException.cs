namespace Cstl.IndoorPositioning.Abstractions.Exceptions
{
    /// <summary>
    /// Raised when no readings match the configured anchors.
    /// </summary>
    public sealed class NoMatchingBeaconsException : IndoorPositioningException
    {
        /// <summary>Creates a new exception.</summary>
        public NoMatchingBeaconsException()
            : base("No beacon readings could be matched to known anchors. Check the MAC addresses.")
        { }
    }
}
