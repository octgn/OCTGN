using System;
using System.Runtime.Serialization;

namespace Octgn.Sdk.Extensibility
{
    [Serializable]
    internal class UnknownFileTypeException : Exception
    {
        public UnknownFileTypeException() {
        }

        public UnknownFileTypeException(string message) : base(message) {
        }

        public UnknownFileTypeException(string message, Exception innerException) : base(message, innerException) {
        }

        protected UnknownFileTypeException(SerializationInfo info, StreamingContext context) : base(info, context) {
        }
    }
}