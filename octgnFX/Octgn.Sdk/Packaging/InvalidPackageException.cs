using System;
using System.Runtime.Serialization;

namespace Octgn.Sdk.Packaging
{
    [Serializable]
    internal class InvalidPackageException : Exception
    {
        public InvalidPackageException() {
        }

        public InvalidPackageException(string message) : base(message) {
        }

        public InvalidPackageException(string message, Exception innerException) : base(message, innerException) {
        }

        protected InvalidPackageException(SerializationInfo info, StreamingContext context) : base(info, context) {
        }
    }
}