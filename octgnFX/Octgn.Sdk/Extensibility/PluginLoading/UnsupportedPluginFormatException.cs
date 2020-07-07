using System;
using System.Runtime.Serialization;

namespace Octgn.Sdk.Extensibility.PluginLoading
{
    [Serializable]
    public class UnsupportedPluginFormatException : Exception
    {
        public UnsupportedPluginFormatException() {
        }

        public UnsupportedPluginFormatException(string message) : base(message) {
        }

        public UnsupportedPluginFormatException(string message, Exception innerException) : base(message, innerException) {
        }

        protected UnsupportedPluginFormatException(SerializationInfo info, StreamingContext context) : base(info, context) {
        }
    }
}