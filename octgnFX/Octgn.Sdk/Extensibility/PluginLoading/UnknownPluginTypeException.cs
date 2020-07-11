using System;
using System.Runtime.Serialization;

namespace Octgn.Sdk.Extensibility.PluginLoading
{
    [Serializable]
    internal class UnknownPluginTypeException : Exception
    {
        public UnknownPluginTypeException() {
        }

        public UnknownPluginTypeException(string message) : base(message) {
        }

        public UnknownPluginTypeException(string message, Exception innerException) : base(message, innerException) {
        }

        protected UnknownPluginTypeException(SerializationInfo info, StreamingContext context) : base(info, context) {
        }
    }
}