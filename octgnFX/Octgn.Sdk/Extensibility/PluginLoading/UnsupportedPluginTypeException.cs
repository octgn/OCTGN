using System;
using System.Runtime.Serialization;

namespace Octgn.Sdk.Extensibility.PluginLoading
{
    [Serializable]
    public class UnsupportedPluginTypeException : Exception
    {
        public UnsupportedPluginTypeException() {
        }

        public UnsupportedPluginTypeException(string message) : base(message) {
        }

        public UnsupportedPluginTypeException(string message, Exception innerException) : base(message, innerException) {
        }

        protected UnsupportedPluginTypeException(SerializationInfo info, StreamingContext context) : base(info, context) {
        }
    }
}