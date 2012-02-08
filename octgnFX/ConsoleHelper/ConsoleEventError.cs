using System;
using System.Xml.Serialization;

namespace Skylabs.ConsoleHelper
{
    [Serializable]
    public class ConsoleEventError : ConsoleEvent
    {
        private ConsoleEventError()
        {
        }

        public ConsoleEventError(String message, Exception e)
            : base(message)
        {
            Header = "!Error: ";
            //Color = ConsoleColor.Red;
            Exception = e;
        }

        [XmlElement("ExceptionMessage")]
        public String ExceptionMessage
        {
            get
            {
                return Exception != null ? Exception.Message : "No Exception Message Data.";
            }
        }

        [XmlElement("StackTrace")]
        public String StackTrace
        {
            get
            {
                return Exception.StackTrace ?? "No Stack Trace Message Data.";
            }
        }

        [XmlIgnore]
        public Exception Exception { get; set; }
    }
}