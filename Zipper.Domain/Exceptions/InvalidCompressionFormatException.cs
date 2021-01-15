using System;

namespace Zipper.Domain.Exceptions
{
    public class InvalidCompressionFormatException : SystemException
    {
        private const string DefaultMessage = "Invalid compression format, are you using same reader/writer/[de]compressor type?";
        
        public InvalidCompressionFormatException()
        {
        }

        public InvalidCompressionFormatException(string message = DefaultMessage) : base(message)
        {
        }

        public InvalidCompressionFormatException(Exception inner, string message = DefaultMessage) : base(message, inner)
        {
        }
    }
}