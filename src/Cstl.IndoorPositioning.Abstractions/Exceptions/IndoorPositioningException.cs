namespace Cstl.IndoorPositioning.Abstractions.Exceptions
{
    /// <summary>
    /// Base exception for indoor positioning pipeline failures.
    /// </summary>
    public class IndoorPositioningException : Exception
    {
        /// <summary>Creates a new exception.</summary>
        public IndoorPositioningException() { }

        /// <summary>Creates a new exception with a message.</summary>
        public IndoorPositioningException(string message)
            : base(message)
        { }

        /// <summary>Creates a new exception with a message and inner exception.</summary>
        public IndoorPositioningException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
