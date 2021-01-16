using System;

namespace Zipper.Domain.Exceptions
{
    public class InvalidFormatException : SystemException
    {
        private const string DefaultMessage = "Invalid compression format, are you using same reader/writer/converter type?";
        
        public InvalidFormatException()
        {
        }

        public InvalidFormatException(string message = DefaultMessage) : base(message)
        {
        }

        public InvalidFormatException(Exception inner, string message = DefaultMessage) : base(message, inner)
        {
        }
    }
}