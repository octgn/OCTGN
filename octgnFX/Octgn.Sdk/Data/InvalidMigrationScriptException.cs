using System;
using System.Runtime.Serialization;

namespace Octgn.Sdk.Data
{
    [Serializable]
    internal class InvalidMigrationScriptException : Exception
    {
        public InvalidMigrationScriptException() {
        }

        public InvalidMigrationScriptException(string message) : base(message) {
        }

        public InvalidMigrationScriptException(string message, Exception innerException) : base(message, innerException) {
        }

        protected InvalidMigrationScriptException(SerializationInfo info, StreamingContext context) : base(info, context) {
        }
    }
}