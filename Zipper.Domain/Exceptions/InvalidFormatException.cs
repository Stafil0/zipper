using System;

namespace Zipper.Domain.Exceptions
{
    /// <summary>
    /// The exception that is thrown when data has invalid read/write format.
    /// </summary>
    public class InvalidFormatException : SystemException
    {
        private const string DefaultMessage = "Invalid compression format, are you using same reader/writer/converter type?";

        /// <summary>
        /// Initialize exception instance.
        /// </summary>
        public InvalidFormatException()
        {
        }

        /// <summary>
        /// Initialize exception instance with custom message.
        /// </summary>
        /// <param name="message">Custom message.</param>
        public InvalidFormatException(string message = DefaultMessage) : base(message)
        {
        }

        /// <summary>
        /// Initialize exception with custom message and inner exception.
        /// </summary>
        /// <param name="inner">Inner exception.</param>
        /// <param name="message">Custom message.</param>
        public InvalidFormatException(Exception inner, string message = DefaultMessage) : base(message, inner)
        {
        }
    }
}