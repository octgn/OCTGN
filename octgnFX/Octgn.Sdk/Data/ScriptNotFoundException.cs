using System;
using System.Runtime.Serialization;

namespace Octgn.Sdk.Data
{
    [Serializable]
    internal class ScriptNotFoundException : Exception
    {
        public ScriptNotFoundException() {
        }

        public ScriptNotFoundException(string message) : base(message) {
        }

        public ScriptNotFoundException(string message, Exception innerException) : base(message, innerException) {
        }

        protected ScriptNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) {
        }
    }
}