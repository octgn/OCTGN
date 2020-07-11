using System;
using System.Runtime.Serialization;

namespace Octgn.Sdk.Extensibility.PluginLoading
{
    [Serializable]
    public class AlreadyInitializedException : Exception
    {
        public AlreadyInitializedException() {
        }

        public AlreadyInitializedException(string message) : base(message) {
        }

        public AlreadyInitializedException(string message, Exception innerException) : base(message, innerException) {
        }

        protected AlreadyInitializedException(SerializationInfo info, StreamingContext context) : base(info, context) {
        }
    }
}