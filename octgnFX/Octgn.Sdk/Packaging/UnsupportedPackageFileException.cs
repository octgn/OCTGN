using System;
using System.Runtime.Serialization;

namespace Octgn.Sdk.Packaging
{
    [Serializable]
    internal class UnsupportedPackageFileException : Exception
    {
        public UnsupportedPackageFileException() {
        }

        public UnsupportedPackageFileException(string message) : base(message) {
        }

        public UnsupportedPackageFileException(string message, Exception innerException) : base(message, innerException) {
        }

        protected UnsupportedPackageFileException(SerializationInfo info, StreamingContext context) : base(info, context) {
        }
    }
}