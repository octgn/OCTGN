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
                if (Exception != null)
                    return Exception.Message;
                return "No Exception Message Data.";
            }
        }

        [XmlElement("StackTrace")]
        public String StackTrace
        {
            get
            {
                if (Exception.StackTrace != null)
                    return Exception.StackTrace;
                return "No Stack Trace Message Data.";
            }
        }

        [XmlIgnore]
        public Exception Exception { get; set; }
    }
}